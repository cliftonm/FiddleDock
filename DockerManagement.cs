using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using FiddleDock.RouteHandlers;

namespace FiddleDock
{
	public abstract class DockerManagement : Response
	{
		// Remember that these fields are specific to the INSTANCE of RunOnDocker, and a new instance is created for each request.
		// The web router calls, for the request:
		//    Execute
		//    GetResponseData
		// for a specific instance of this class, so we can preserve things like the 
		// instancePort within the context of the instance of this class.
		protected Session session;
		protected int instancePort;

		// !!! This however is global to the server, as we need to track all ports used across all sessions.
		private static List<int> globalUsedPorts = new List<int>();

		/// <summary>
		/// Returns null if no ports are in use by this session, otherwise the list of ports.
		/// </summary>
		protected List<int> GetSessionDockerInstancePorts()
		{
			List<int> usedPorts = session.GetObject<List<int>>("usedPorts");

			return usedPorts;
		}

		protected void SaveSessionDockerInstancePorts(List<int> ports)
		{
			session["usedPorts"] = ports;
		}

		protected int GetOrCreateContainerPort(ref List<int> sessionPorts, int instanceNumber)
		{
			int port;

			if (sessionPorts == null)
			{
				port = CreateContainer();
				sessionPorts = new List<int>(new int[instanceNumber + 1]);
				sessionPorts[instanceNumber] = port;
			}
			else
			{
				port = sessionPorts[instanceNumber];

				if (port == 0)
				{
					// Oops, we haven't actually created this container.  This occurs when:
					// The user creates a new instance
					// The user selects the new instance
					// The user goes back to instance 1 (index 0) which has not been used yet!
					// Basically, I boxed myself into a corner by not creating the first Docker instance, so we have
					// some crufty code here as a result.

					port = CreateContainer();
					sessionPorts[instanceNumber] = port;
				}
			}

			return port;
		}

		/// <summary>
		/// I boxed myself into a corner by not creating the first Docker instance, so we have
		/// some crufty code here as a result.
		/// </summary>
		protected void UpdateSessionPort(List<int> sessionPorts, int instanceNumber, int port)
		{
			if (sessionPorts.Count == instanceNumber)
			{
				sessionPorts.Add(port);
			}
			else
			{
				sessionPorts[instanceNumber] = port;
			}
		}

		protected void DeleteContainerPort(ref List<int> sessionPorts, int instanceNumber)
		{
			int port = sessionPorts[instanceNumber];
			ExitContainer(port);
			sessionPorts.RemoveAt(instanceNumber);
		}

		protected int CreateContainer()
		{
			List<string> stdout = new List<string>();
			int port = GetAvailablePort();
			SaveStdout(stdout, port);
			string parms = String.Format("run -p {0}:{0} fiddlepy python -u server.py -p {0}", port);
			var process = Runner.LaunchProcess("docker", parms, (s) => stdout.Add(s), (err) => stdout.Add(err));
			string resp;

			try
			{
				resp = WaitForDockerImage(port);
				VerifyResponse(resp, "Hello World!");
			}
			catch (Exception ex)
			{
				stdout.Add(ex.Message);
			}


			return port;
		}

		protected List<string> GetStdout()
		{
			return session.GetObject<List<string>>(instancePort.ToString());
		}

		protected void SaveStdout(List<string> stdout, int port)
		{
			session[port.ToString()] = stdout;
		}

		protected int GetAvailablePort()
		{
			int newPort;

			if (globalUsedPorts.Count == 0)
			{
				newPort = 1001;
				globalUsedPorts.Add(newPort);
			}
			else
			{
				newPort = globalUsedPorts.DefaultIfEmpty(0).Max() + 1;
				globalUsedPorts.Add(newPort);
			}

			return newPort;
		}

		protected string WaitForDockerImage(int port)
		{
			string url = GetUrl(port, "/");
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				using (Stream stream = response.GetResponseStream())
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string resp = reader.ReadToEnd();

						return resp;
					}
				}
			}
		}

		protected string GetUrl(int port, string cmd)
		{
			return String.Format("{0}:{1}{2}", Constants.DOCKER_IP, port, cmd);
		}

		protected void VerifyResponse(string resp, string expectedResponse)
		{
			if (resp != expectedResponse)
			{
				throw new Exception("Did not get expected response: " + resp);
			}
		}

		protected string UploadApp(string code)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(GetUrl(instancePort, "/uploadfile"));
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				string json = "{\"Filename\":\"run.py\"," +
							  "\"Content\":\"" + code + "\", " +
							  "\"Encoding\":\"base64\"}";

				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();
			}

			var httpResponse = httpWebRequest.GetResponse();

			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var result = streamReader.ReadToEnd();

				return result;
			}
		}

		protected string RunApp()
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(GetUrl(instancePort, "/run"));
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				string json = "{\"Filename\":\"run.py\"}";
				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();
			}

			var httpResponse = httpWebRequest.GetResponse();

			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var result = streamReader.ReadToEnd();

				return result;
			}
		}

		protected void ExitContainer(int port)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(GetUrl(port, "/exit"));
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			try { httpWebRequest.GetResponse(); } catch { }     // container exits and does not send a response.
		}

		protected void ResetStdout(string resp)
		{
			List<string> stdout = GetStdout();
			stdout.Clear();
			var ret = (JArray)JsonConvert.DeserializeObject(resp);
			stdout.AddRange(ret.Select(t => t.ToString().Trim()));
		}
	}
}
