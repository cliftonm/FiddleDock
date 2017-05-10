using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace FiddleDock.RouteHandlers
{
	public class RunOnDocker : DockerManagement
	{
		public RunOnDocker(Session session)
		{
			this.session = session;
		}

		public override void Execute(string requestData)
		{
			PythonCode pcode = JsonConvert.DeserializeObject<PythonCode>(requestData);
			int instanceNumber = pcode.InstanceNumber - 1;
			var sessionPorts = GetSessionDockerInstancePorts();
			instancePort = GetOrCreateContainerPort(ref sessionPorts, instanceNumber);
			SaveSessionDockerInstancePorts(sessionPorts);       // Update with any new ports created in this session.
			List<string> stdout = GetStdout();

			string code = pcode.Code;			// This is already base64 encoded.
			string resp = String.Empty;

			try
			{
				resp = UploadApp(code);
				VerifyResponse(resp, "ok");
				resp = RunApp();
				ResetStdout(resp);
			}
			catch (Exception ex)
			{
				stdout.Add(ex.Message);
			}

			// ExitContainer();
		}

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			List<string> stdout = GetStdout();
			string resp = Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Join("\r\n", stdout)));
			return Encoding.UTF8.GetBytes("{\"status\":\"ok\", \"resp\":\"" + resp + "\"}");
		}
	}
}
