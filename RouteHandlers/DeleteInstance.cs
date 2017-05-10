using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace FiddleDock.RouteHandlers
{
	public class DeleteInstance : DockerManagement
	{
		public DeleteInstance(Session session)
		{
			this.session = session;
		}

		public override void Execute(string requestData)
		{
			PythonCode pcode = JsonConvert.DeserializeObject<PythonCode>(requestData);
			int instanceNumber = pcode.InstanceNumber - 1;
			var sessionPorts = GetSessionDockerInstancePorts();
			DeleteContainerPort(ref sessionPorts, instanceNumber);
			SaveSessionDockerInstancePorts(sessionPorts);       // Update with any new ports created in this session.
		}

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			return Encoding.UTF8.GetBytes("{\"status\":\"ok\"}");
		}
	}
}
