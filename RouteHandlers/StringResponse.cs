using System.Net;
using System.Text;

namespace FiddleDock.RouteHandlers
{
	public class StringResponse : Response
	{
		public string Data { get; set; }

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			byte[] utf8data = Encoding.UTF8.GetBytes(Data);
			context.Response.ContentEncoding = Encoding.UTF8;

			return utf8data;
		}
	}
}
