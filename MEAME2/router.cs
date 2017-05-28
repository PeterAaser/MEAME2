using Nancy;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Nancy.ModelBinding;
using Nancy.Validation;
using System;
using System.IO;
using Newtonsoft.Json;


namespace MEAME2
{
  public class MEAMEserver : NancyModule
  {
    private MEAMEcontrol controller;

    public MEAMEserver(MEAMEcontrol controller){

      this.controller = controller;

      Get["/status"] = _ => this.status();
      Get["/"] = _ => "welcome to MEAME";

      Post["/DAQ/connect"] = _ => connectDAQ();
      Post["/DAQ/start"] = _ => startDAQ();
      Post["/DAQ/stop"] = _ => stopDAQ();

    }


    private string getJsonBody(){
      var id = this.Request.Body;
      long length = this.Request.Body.Length;
      byte[] data = new byte[length];
      id.Read(data, 0, (int)length);
      string body = System.Text.Encoding.Default.GetString(data);
      return body;
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


    private dynamic connectDAQ(){
      string body = this.getJsonBody();
      Console.WriteLine(body);

      StringReader memeReader = new StringReader(body);
      JsonTextReader memer = new JsonTextReader(memeReader);
      JsonSerializer serializer = new JsonSerializer();
      DAQconfig d = serializer.Deserialize<DAQconfig>(memer);

      bool connect = controller.connectDAQ(d);

      if (connect){
        return 200;
      }

      return 500; // what if it's just a generic error? dunno lol use remmina...
    }


    private dynamic startDAQ(){
      if (controller.startServer())
        {
          return 200;
        }

      return 500;
    }


    private dynamic stopDAQ(){
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
