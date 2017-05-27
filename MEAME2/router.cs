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
      return "all devices found ayy hurr xD";
    }


    private dynamic connectDAQ(){
      string body = this.getJsonBody();
      Console.WriteLine(body);

      StringReader memeReader = new StringReader(body);
      JsonTextReader memer = new JsonTextReader(memeReader);
      JsonSerializer serializer = new JsonSerializer();
      DAQconfig s = serializer.Deserialize<DAQconfig>(memer);

      Console.WriteLine(s);
      Console.WriteLine(s.samplerate);
      Console.WriteLine(s.segmentLength);

      return 200;
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

    private dynamic memeShit(dynamic parameters)
    {
      var id = this.Request.Body;
      long length = this.Request.Body.Length;
      byte[] data = new byte[length];
      id.Read(data, 0, (int)length);
      string body = System.Text.Encoding.Default.GetString(data);
      var p = body.Split('&')
        .Select(s => s.Split('='))
        .ToDictionary(k => k.ElementAt(0), v => v.ElementAt(1));

      if (p["username"] == "volkan")
        return "awesome!";
      else
        return "meh!";
    }
  }
}
