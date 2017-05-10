using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FiddleDock.RouteHandlers
{
	public class RunOnDocker : Response
	{
		protected Session session;

		public RunOnDocker(Session session)
		{
			this.session = session;
		}

		protected List<string> stdout = new List<string>();

		public override void Execute(string requestData)
		{
			PythonCode pcode = JsonConvert.DeserializeObject<PythonCode>(requestData);
			string code = pcode.Code;			// This is already base64 encoded.
			var process = Runner.LaunchProcess("docker",  "run -p 1005:1005 fiddlepy python -u server.py -p 1005", (s) => stdout.Add(s), (err) => stdout.Add(err));
			string resp = String.Empty;

			try
			{
				resp = WaitForDockerImage();
				VerifyResponse(resp, "Hello World!");
				resp = UploadApp(code);
				VerifyResponse(resp, "ok");
				resp = RunApp();
				ResetStdout(resp);
			}
			catch (Exception ex)
			{
				stdout.Add(ex.Message);
			}

			ExitContainer();
		}

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			string resp = Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Join("\r\n", stdout)));
			return Encoding.UTF8.GetBytes("{\"status\":\"ok\", \"resp\":\"" + resp + "\"}");
		}

		protected string WaitForDockerImage()
		{
			string url = "http://192.168.99.100:1005/";
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

		protected void VerifyResponse(string resp, string expectedResponse)
		{
			if (resp != expectedResponse)
			{
				throw new Exception("Did not get expected response: " + resp);
			}
		}

		protected string UploadApp(string code)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.99.100:1005/uploadfile");
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
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.99.100:1005/run");
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

		protected void ExitContainer()
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://192.168.99.100:1005/exit");
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			try { httpWebRequest.GetResponse(); } catch { }		// container exits and does not send a response.
		}

		protected void ResetStdout(string resp)
		{
			stdout.Clear();
			var ret = (JArray)JsonConvert.DeserializeObject(resp);
			stdout.AddRange(ret.Select(t => t.ToString().Trim()));
		}
	}
}
