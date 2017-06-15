using Nancy;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Nancy.ModelBinding;
using Nancy.Validation;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using System;
using System.IO;
using Newtonsoft.Json;


namespace MEAME2
{
  public class Bootstrapper : DefaultNancyBootstrapper
  {
    protected override void ConfigureApplicationContainer(TinyIoCContainer container)
    {
      base.ConfigureApplicationContainer(container);
      // Autoregister will actually do this for us, so we don't need this line,
      // but I'll keep it here to demonstrate. By Default anything registered
      // against an interface will be a singleton instance.

      container.Register<IMEAMEcontrol, MEAMEcontrol>().AsSingleton();
    }
  }

  public class MEAMEserver : NancyModule
  {


    public IMEAMEcontrol controller;

    public MEAMEserver(IMEAMEcontrol controller){

      this.controller = controller;

      Get["/status"] = _ => this.hello();
      Get["/"] = _ => "hello this is MEAME.";

      Post["/DAQ/connect"] = _ => connectDAQ();
      Get["/DAQ/start"] = _ => startDAQ();
      Get["/DAQ/stop"] = _ => stopDAQ();

    }


    private string getJsonBody()
    {
      var id = this.Request.Body;
      long length = this.Request.Body.Length;
      byte[] data = new byte[length];
      id.Read(data, 0, (int)length);
      string body = System.Text.Encoding.Default.GetString(data);
      return body;
    }


    private dynamic hello(){
      Console.WriteLine("\n---[Runnin Hello]---");
      Console.WriteLine("Hello :DD");
      Console.WriteLine("---[ Hello :) ]---");
      return "Hello :D";
    }


    private dynamic status()
    {
      string meme = controller.getDevicesDescription();
      byte[] jsonBytes = Encoding.UTF8.GetBytes(meme);

      return new Response()
        {
          StatusCode = HttpStatusCode.OK,
          ContentType = "application/json",
          ReasonPhrase = "here's some fucking data",
          Headers = new Dictionary<string, string>()
          {
            { "Content-Type", "application/json" },
            { "X-Custom-Header", "heyyy gamers" }
          },
          Contents = c => c.Write(jsonBytes, 0, jsonBytes.Length)
        };
    }


    // Requires a JSON in the body
    private dynamic connectDAQ(){

      Console.WriteLine("\n---[Runnin connectDAQ]---");

      string body = this.getJsonBody();
      Console.WriteLine(body);

      try {
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        DAQconfig d = serializer.Deserialize<DAQconfig>(memer);

        bool connect = controller.connectDAQ(d);

        if (connect){
          Console.WriteLine("---[200]---");
          return 200;
        }
        Console.WriteLine("---[ERROR: connectDAQ failed]---");
        return 500; // what if it's just a generic error? dunno lol use remmina...
      }
      catch (Exception e){ // should only catch deserialize error, dunno how xD
        Console.WriteLine("malformed request");
        Console.WriteLine("---[ERROR: Malformed request]---");
        Console.WriteLine(e);
        Console.WriteLine("---[ERROR: Malformed request]---");
        return 500;
      }


    }


    private dynamic startDAQ(){
      Console.WriteLine("\n---[Runnin startDAQ]---");

      if (controller.startServer())
        {
          Console.WriteLine("---[200]---");
          return 200;
        }

      Console.WriteLine("---[ERROR: Something wrong with startServer]---");
      return 500;
    }


    private dynamic stopDAQ(){

      Console.WriteLine("Runnin stopDAQ");

      if (controller.stopServer())
        {
          return 200;
        }

      return 500;
    }

    private dynamic something(dynamic parameters)
    {
      string jsonString = "{ username: \"admin\", password: \"just kidding\" }";
      byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

      return new Response()
        {
          StatusCode = HttpStatusCode.OK,
          ContentType = "application/json",
          ReasonPhrase = "Because why not!",
          Headers = new Dictionary<string, string>()
          {
            { "Content-Type", "application/json" },
            { "X-Custom-Header", "Sup?" }
          },
          Contents = c => c.Write(jsonBytes, 0, jsonBytes.Length)
        };
    }
  }
}
