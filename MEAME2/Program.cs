using System;
using Nancy.Hosting.Self;
using System.Diagnostics;
using System.Threading;

namespace MEAME2
{
		class MainClass
		{
				public static void Main (string[] args)
				{
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine ("STARTING MEAME SERVER...");

						var nancyHost = new NancyHost(new Uri("http://localhost:8888/"));

						nancyHost.Start();

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("MEAME is now listenin'");
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.ResetColor();
						Console.ReadKey();
						nancyHost.Stop();
						Console.WriteLine("Stopped, see ya!");

						Environment.Exit(0);
				}
		}
}
