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
      consoleInfo("Http hello request");
      consoleOK("Hello :D");
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

      consoleInfo("Got request for DAQ connect");

      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        DAQconfig d = serializer.Deserialize<DAQconfig>(memer);
        consoleInfo($"With parameters:");
        consoleInfo($"samplerate:\t{d.samplerate}");
        consoleInfo($"segment length:\t{d.segmentLength}");

        bool connect = controller.connectDAQ(d);

        if (connect){
          consoleOK("DAQ connected");
          return 200;
        }
        consoleError("Connecting to DAQ failed");
        return 500; // what if it's just a generic error? dunno lol use remmina...
      }
      catch (Exception e){ // should only catch deserialize error, dunno how xD
        consoleError("malformed request");
        Console.WriteLine(e);
        return 500;
      }


    }


    private dynamic startDAQ(){
      consoleInfo("Got request for DAQ start");

      if (controller.startServer())
        {
          consoleOK("DAQ server started");
          return 200;
        }

      consoleError("DAQ server failed to start");
      return 500;
    }


    private dynamic stopDAQ(){
      consoleInfo("Got request to stop DAQ");

      if (controller.stopServer())
        {
          consoleOK("DAQ stopped");
          return 200;
        }

      consoleError("Unable to stop DAQ signalled by DAQ stop return value");
      return 500;
    }



    private void consoleError(String s){
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"[Error]: {s}");
      Console.ResetColor();
    }

    private void consoleInfo(String s){
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine($"[Info]: {s}");
      Console.ResetColor();
    }

    private void consoleOK(String s){
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"[Info]: {s}\n\n");
      Console.ResetColor();
    }
  }
}
