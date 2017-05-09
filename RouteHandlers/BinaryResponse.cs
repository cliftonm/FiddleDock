using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FiddleDock.RouteHandlers
{
	public class BinaryResponse : Response
	{
		public byte[] ByteData { get; set; }

		public override byte[] GetResponseData(HttpListenerContext context)
		{
			return ByteData;
		}
	}
}
