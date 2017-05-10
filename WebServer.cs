using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Clifton.Core.ExtensionMethods;

using FiddleDock.RouteHandlers;

namespace FiddleDock
{
	public class LogEventArgs : EventArgs
	{
		public string Message { get; set; }
	}

	public class WebServer
	{
		public event EventHandler<LogEventArgs> Logger;

		protected int maxSimultaneousConnections = 20;
		protected int httpPort = 80;
		protected Semaphore sem;
		protected Dictionary<Route, Func<HttpListenerContext, Session, Response>> routeHandlers;
		protected SessionManager sessionManager;

		public WebServer()
		{
			routeHandlers = new Dictionary<Route, Func<HttpListenerContext, Session, Response>>();
			sessionManager = new SessionManager();
		}

		public void AddRoute(string verb, string path, Func<HttpListenerContext, Session, Response> handler)
		{
			routeHandlers.Add(new Route(verb, path), handler);
		}

		public void StartWebServer()
		{
			sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
			List<IPAddress> localHostIPs = GetLocalHostIPs();
			HttpListener listener = InitializeListener(localHostIPs);
			Start(listener);
		}

		protected void Start(HttpListener listener)
		{
			listener.Start();
			Thread th = new Thread(RunServer);
			th.IsBackground = true;
			th.Start(listener);
		}

		protected void RunServer(object l)
		{
			HttpListener listener = (HttpListener)l;
			while (true)
			{
				try
				{
					sem.WaitOne();
					StartConnectionListener(listener);
				}
				catch (Exception ex)
				{
					Logger.Fire(this, new LogEventArgs() { Message = ex.Message });
				}
			}
		}

		protected List<IPAddress> GetLocalHostIPs()
		{
			IPHostEntry host;
			host = Dns.GetHostEntry(Dns.GetHostName());
			List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

			return ret;
		}

		protected HttpListener InitializeListener(List<IPAddress> localhostIPs)
		{
			HttpListener listener = new HttpListener();
			Logger.Fire(this, new LogEventArgs() { Message = "Listening on IP " + "http://locahost:" + httpPort + "/" });
			listener.Prefixes.Add("http://localhost:" + httpPort + "/");

			// Listen to IP address as well.
			localhostIPs.ForEach(ip =>
			{
				Logger.Fire(this, new LogEventArgs() { Message = "Listening on IP " + "http://" + ip.ToString() + ":" + httpPort + "/" });
				listener.Prefixes.Add("http://" + ip.ToString() + ":" + httpPort + "/");
			});

			return listener;
		}

		protected void StartConnectionListener(HttpListener listener)
		{
			// Wait for a connection. Return to caller while we wait.
			HttpListenerContext context = listener.GetContext();

			// Release the semaphore so that another listener can be immediately started up.
			sem.Release();

			Logger.Fire(this, new LogEventArgs() { Message = context.Request.Url.LocalPath });

			Session session = sessionManager.AddSessionIfNew(context.Request.RemoteEndPoint.Address);

			string verb = context.Request.HttpMethod;
			string path = context.Request.Url.LocalPath;
			string requestData = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

			var routes = routeHandlers.Where(kvp => kvp.Key.IsMatch(verb, path));
			int numRoutes = routes.Count();

			if (numRoutes == 0)
			{
				Console.WriteLine("Route not found!");
				Respond(context, "<p>Route not found!</p>", "text/html");
			}
			else if (numRoutes > 1)
			{
				Console.WriteLine("Multiple handlers match the given route!");
				Respond(context, "<p>Multiple handlers match the given route!</p>", "text/html");
			}
			else
			{

				try
				{
					Response response = routes.First().Value(context, session);
					response.Execute(requestData);
					Respond(context, response);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Respond(context, "error", "text/html");
				}
			}
		}

		protected void Respond(HttpListenerContext context, Response response)
		{
			context.Response.ContentType = response.ContentType;
			var data = response.GetResponseData(context);
			context.Response.ContentLength64 = data.Length;
			context.Response.OutputStream.Write(data, 0, data.Length);
		}

		protected void Respond(HttpListenerContext context, string msg, string contentType)
		{
			byte[] utf8data = Encoding.UTF8.GetBytes(msg);
			context.Response.ContentType = contentType;
			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.ContentLength64 = utf8data.Length;
			context.Response.OutputStream.Write(utf8data, 0, utf8data.Length);
		}
	}
}