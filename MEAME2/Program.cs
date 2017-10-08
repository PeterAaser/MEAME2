using System;
using Nancy.Hosting.Self;
using System.Diagnostics;

namespace MEAME2
{
  class MainClass
  {
    public static void Main (string[] args)
    {
      AppDomain.CurrentDomain.ProcessExit += new EventHandler (OnProcessExit); 

      Console.WriteLine ("STARTING MEAME SERVER");

      var nancyHost = new NancyHost(new Uri("http://localhost:8888/"));

      nancyHost.Start();

      Console.WriteLine("MEAME is now listenin'");
      Console.ReadKey();
      nancyHost.Stop();
      Console.WriteLine("Stopped, see ya!");
    }


    public static void Server (string[] args){
      AppDomain.CurrentDomain.ProcessExit += new EventHandler (OnProcessExit); 

      Console.WriteLine ("STARTING MEAME SERVER");

      var nancyHost = new NancyHost(new Uri("http://localhost:8888/"));

      nancyHost.Start();

      Console.WriteLine("MEAME is now listenin'");
      Console.ReadKey();
      nancyHost.Stop();
      Console.WriteLine("Stopped, see ya!");
    }

    static void OnProcessExit (object sender, EventArgs e)
    {
      Console.WriteLine ("I'm out of here");
    }


    public static void notServer (string[] args){
      Console.WriteLine("Auxilliary test method");

      DSPComms dspComms = new DSPComms();
      dspComms.init();

      Console.WriteLine("Done, press any key to exit");
      Console.ReadKey();
    }
  }
}
