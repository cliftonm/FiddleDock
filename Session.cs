using System.Collections.Generic;

namespace FiddleDock
{
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
