using System.Collections.Generic;
using System.Net;

namespace FiddleDock
{
	/// <summary>
	/// Scary session manager because a session never expires.
	/// </summary>
	public class SessionManager
	{
		public Dictionary<IPAddress, Session> sessionMap;

		public SessionManager()
		{
			sessionMap = new Dictionary<IPAddress, Session>();
		}

		public Session AddSessionIfNew(IPAddress addr)
		{
			Session session;

			if (!sessionMap.TryGetValue(addr, out session))
			{
				session = new Session();
				sessionMap[addr] = session;
			}

			return session;
		}
	}
}


