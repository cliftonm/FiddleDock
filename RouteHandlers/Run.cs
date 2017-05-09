using System;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace FiddleDock.RouteHandlers
{
	public class Run : Response
	{
		public override byte[] GetResponseData(HttpListenerContext context)
		{
			return Encoding.UTF8.GetBytes("ok");
		}

		public override void Execute(string requestData)
		{
			PythonCode pcode = JsonConvert.DeserializeObject<PythonCode>(requestData );
			string code = Encoding.ASCII.GetString(Convert.FromBase64String(pcode.Code));
		}
	}
}
