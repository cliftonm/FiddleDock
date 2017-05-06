using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddleDock
{
	public class Response
	{
		public string Data { get; set; }
		public byte[] ByteData { get; set; }
		public string ContentType { get; set; }
	}
}
