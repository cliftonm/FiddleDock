using System;
using System.Diagnostics;

using System.Collections.Generic;
using System.Linq;

namespace FiddleDock
{
	public static class Runner
	{
		public static Process LaunchProcess(string processName, string arguments, Action<string> onOutput, Action<string> onError = null)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.FileName = processName;
			p.StartInfo.Arguments = arguments;
			p.StartInfo.CreateNoWindow = true;

			p.StartInfo.EnvironmentVariables["DOCKER_CERT_PATH"] = @"c:\users\Marc\.docker\machine\machines\default";
			p.StartInfo.EnvironmentVariables["DOCKER_HOST"] = "tcp://192.168.99.100:2376";
			p.StartInfo.EnvironmentVariables["DOCKER_MACHINE_NAME"] = "default";
			p.StartInfo.EnvironmentVariables["DOCKER_TLS_VERIFY"] = "1";

			p.OutputDataReceived += (sndr, args) => { if (args.Data != null) onOutput(args.Data); };

			if (onError != null)
			{
				p.ErrorDataReceived += (sndr, args) => { if (args.Data != null) onError(args.Data); };
			}

			p.Start();

			// Interestingly, this has to be called after Start().
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();

			return p;
		}
	}
}
