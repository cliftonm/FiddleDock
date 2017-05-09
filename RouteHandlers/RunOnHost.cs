using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

namespace FiddleDock.RouteHandlers
{
	public class RunOnHost : Response
	{
		protected List<string> stdout = new List<string>();

		public override void Execute(string requestData)
		{
			PythonCode pcode = JsonConvert.DeserializeObject<PythonCode>(requestData);
			string code = Encoding.ASCII.GetString(Convert.FromBase64String(pcode.Code));
			string fnTemp = Path.GetTempFileName();
			File.WriteAllText(fnTemp, code);
			var process = Runner.LaunchProcess("python", "-u " + fnTemp, (s) => stdout.Add(s), (err) => stdout.Add(err));
			var startTime = DateTime.Now;

			while (!process.HasExited && (DateTime.Now - startTime).TotalMilliseconds < Constants.MAX_RUN_TIME_MS)
			{
				Thread.Sleep(0);
			}

			if (!process.HasExited)
			{
				process.Kill();
			}

			File.Delete(fnTemp);
		}

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			string resp = Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Join("\r\n", stdout)));
			return Encoding.UTF8.GetBytes("{\"status\":\"ok\", \"resp\":\"" + resp + "\"}");
		}
	}
}
