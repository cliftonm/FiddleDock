using System.Text.RegularExpressions;

namespace FiddleDock
{
	public class Route
	{
		public string Verb { get; protected set; }
		public string Path { get; protected set; }

		protected Regex regex;

		public Route(string verb, string path)
		{
			Verb = verb.ToUpper();
			Path = path.ToLower();

			regex = new Regex(WildcardToRegex(path), RegexOptions.IgnoreCase);
		}

		public bool IsMatch(string verb, string path)
		{
			return Verb == verb.ToUpper() && regex.IsMatch(path);
		}

		protected string WildcardToRegex(string pattern)
		{
			return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		}
	}

}
