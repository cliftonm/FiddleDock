using System.Collections.Generic;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace FiddleDock.RouteHandlers
{
	public class CreateNewInstance : DockerManagement
	{
		public CreateNewInstance(Session session)
		{
			this.session = session;
		}

		public override void Execute(string requestData)
		{
			PythonCode pcode = JsonConvert.DeserializeObject<PythonCode>(requestData);
			int instanceNumber = pcode.InstanceNumber - 1;
			var sessionPorts = GetSessionDockerInstancePorts() ?? new List<int>(new int[instanceNumber + 1]);
			int port = CreateContainer();
			UpdateSessionPort(sessionPorts, instanceNumber, port);
			SaveSessionDockerInstancePorts(sessionPorts);       // Update with any new ports created in this session.
		}

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			return Encoding.UTF8.GetBytes("{\"status\":\"ok\"}");
		}
	}
}
