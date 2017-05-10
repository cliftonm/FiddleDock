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

	public class Session
	{
		protected Dictionary<string, object> Objects;

		public object this[string objectKey]
		{
			get
			{
				object val = null;
				Objects.TryGetValue(objectKey, out val);

				return val;
			}

			set { Objects[objectKey] = value; }
		}

		public T GetObject<T>(string objectKey)
		{
			object val = null;
			T ret = default(T);

			if (Objects.TryGetValue(objectKey, out val))
			{
				ret = (T)val;
			}

			return ret;
		}

		public Session()
		{
			Objects = new Dictionary<string, object>();
		}
	}
}


