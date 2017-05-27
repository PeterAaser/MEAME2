using System;
using Nancy.Hosting.Self;
using System.Diagnostics;

namespace MEAME2
{
  class MainClass
  {
    public static void Main (string[] args)
    {

      Console.WriteLine ("STARTING MEAME SERVER");
      Console.WriteLine ("Setting up DI stuff");

      var controller = new MEAMEcontrol();

      var nancyHost = new NancyHost(new Uri("http://localhost:8888/"));

      nancyHost.Start();

      Console.WriteLine("MEAME is now listenin'");
      Console.ReadKey();
      nancyHost.Stop();
      Console.WriteLine("Stopped, see ya!");
    }
  }
}
