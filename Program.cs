using System;
using System.IO;

using Clifton.Core.ExtensionMethods;

namespace FiddleDock
{
	class Program
	{
		static void Main(string[] args)
		{
			WebServer ws = new WebServer();
			ws.Logger += Logger;
			InitializeRoutes(ws);
			ws.StartWebServer();
			Console.WriteLine("Web server ready.");
			Console.ReadLine();
		}

		private static void Logger(object sender, LogEventArgs e)
		{
			Console.WriteLine(e.Message);
		}

		private static string GetPath()
		{
			return @"c:\projects\FiddleDock\Website";
		}

		private static void InitializeRoutes(WebServer ws)
		{
			ws.AddRoute("GET", "/", (context) => new Response() { Data = File.ReadAllText(Path.Combine(GetPath(), "index.html")), ContentType = "text/html" });
			ws.AddRoute("GET", "/index", (context) => new Response() { Data = File.ReadAllText(Path.Combine(GetPath(), "index.html")), ContentType = "text/html" });
			ws.AddRoute("GET", "/index.html", (context) => new Response() { Data = File.ReadAllText(Path.Combine(GetPath(), "index.html")), ContentType = "text/html" });
			ws.AddRoute("GET", "*.js", (context) => new Response() { Data = File.ReadAllText(Path.Combine(GetPath(), context.Request.Url.LocalPath.WindowsDelimiters().Substring(1))), ContentType = "text/javascript" });
			ws.AddRoute("GET", "*.css", (context) => new Response() { Data = File.ReadAllText(Path.Combine(GetPath(), context.Request.Url.LocalPath.WindowsDelimiters().Substring(1))), ContentType = "text/css" });
			ws.AddRoute("GET", "*.jpg", (context) => new Response() { ByteData = File.ReadAllBytes(Path.Combine(GetPath(), context.Request.Url.LocalPath.WindowsDelimiters().Substring(1))), ContentType = "image/jpg" });
			ws.AddRoute("GET", "*.png", (context) => new Response() { ByteData = File.ReadAllBytes(Path.Combine(GetPath(), context.Request.Url.LocalPath.WindowsDelimiters().Substring(1))), ContentType = "image/png" });
			ws.AddRoute("GET", "*.bmp", (context) => new Response() { ByteData = File.ReadAllBytes(Path.Combine(GetPath(), context.Request.Url.LocalPath.WindowsDelimiters().Substring(1))), ContentType = "image/bmp" });
			ws.AddRoute("GET", "*.ico", (context) => new Response() { ByteData = File.ReadAllBytes(Path.Combine(GetPath(), context.Request.Url.LocalPath.WindowsDelimiters().Substring(1))), ContentType = "image/x-icon" });
		}
	}
}
