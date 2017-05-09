using System.Net;

namespace FiddleDock.RouteHandlers
{
	public abstract class Response
	{
		public string ContentType { get; set; }

		public abstract byte[] GetResponseData(HttpListenerContext context);
		public virtual void Execute(string requestData) { }
	}
}
