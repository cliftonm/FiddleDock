using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Clifton.Core.ExtensionMethods
{
	public static class ExtensionMethods
	{
		public static string LeftOf(this string src, char c)
		{
			string ret = src;
			int idx = src.IndexOf(c);

			if (idx != -1)
			{
				ret = src.Substring(0, idx);
			}

			return ret;
		}

		public static string LeftOf(this String src, string s)
		{
			string ret = src;
			int idx = src.IndexOf(s);

			if (idx != -1)
			{
				ret = src.Substring(0, idx);
			}

			return ret;
		}

		public static string RightOf(this String src, string s)
		{
			string ret = String.Empty;
			int idx = src.IndexOf(s);

			if (idx != -1)
			{
				ret = src.Substring(idx + s.Length);
			}

			return ret;
		}

		public static string LeftOfRightmostOf(this String src, char c)
		{
			string ret = src;
			int idx = src.LastIndexOf(c);

			if (idx != -1)
			{
				ret = src.Substring(0, idx);
			}

			return ret;
		}

		public static string LeftOfRightmostOf(this String src, string s)
		{
			string ret = src;
			int idx = src.IndexOf(s);
			int idx2 = idx;

			while (idx2 != -1)
			{
				idx2 = src.IndexOf(s, idx + s.Length);

				if (idx2 != -1)
				{
					idx = idx2;
				}
			}

			if (idx != -1)
			{
				ret = src.Substring(0, idx);
			}

			return ret;
		}

		public static string RightOfRightmostOf(this string src, char c)
		{
			string ret = String.Empty;
			int idx = src.LastIndexOf(c);

			if (idx != -1)
			{
				ret = src.Substring(idx + 1);
			}

			return ret;
		}

		public static string RightOfRightmostOf(this String src, string s)
		{
			string ret = src;
			int idx = src.IndexOf(s);
			int idx2 = idx;

			while (idx2 != -1)
			{
				idx2 = src.IndexOf(s, idx + s.Length);

				if (idx2 != -1)
				{
					idx = idx2;
				}
			}

			if (idx != -1)
			{
				ret = src.Substring(idx + s.Length, src.Length - (idx + s.Length));
			}

			return ret;
		}

		public static char Rightmost(this String src)
		{
			char c = '\0';

			if (src.Length > 0)
			{
				c = src[src.Length - 1];
			}

			return c;
		}

		public static string Between(this string src, char start, char end)
		{
			string ret = String.Empty;
			int idxStart = src.IndexOf(start);

			if (idxStart != -1)
			{
				++idxStart;
				int idxEnd = src.IndexOf(end, idxStart);

				if (idxEnd != -1)
				{
					ret = src.Substring(idxStart, idxEnd - idxStart);
				}
			}

			return ret;
		}

		public static int CountOf(this String src, char c)
		{
			return src.Count(q => q == c);
		}

		/// <summary>
		/// Encapsulates testing for whether the event has been wired up.
		/// </summary>
		public static void Fire<TEventArgs>(this EventHandler<TEventArgs> theEvent, object sender, TEventArgs e = null) where TEventArgs : EventArgs
		{
			theEvent?.Invoke(sender, e);
		}

		public static string WindowsDelimiters(this string src)
		{
			return src.Replace("/", "\\");
		}

		public static bool Matches(this string str, string pattern)
		{
			if (String.IsNullOrEmpty(pattern))
			{
				return false;
			}

			Regex r = new Regex(WildcardToRegex(pattern), RegexOptions.IgnoreCase);

			return r.IsMatch(str);
		}

		public static string WildcardToRegex(string pattern)
		{
			return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		}
	}
}
