using Nancy;
using Nancy.Extensions;
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
      Post["/logmsg"] = _ => logmsg();

      Post["/DAQ/connect"]       = _ => connectDAQ();
      Get["/DAQ/start"]          = _ => startDAQ();
      Get["/DAQ/stop"]           = _ => stopDAQ();

      Post["/DSP/connect"]       = _ => connectDSP();
      Post["/DSP/setreg"]        = _ => setRegs();
      Post["/DSP/readreg"]       = _ => readRegs();

      Post["/DSP/stimreq"]       = _ => stimReq();
      Post["/DSP/stimtest"]      = _ => stimTest();

      Post["/DSP/dump"]          = _ => stimDump();
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
      log.info("Http hello request");
      log.ok("Hello :D");
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


    private dynamic logmsg(){
      log.info(this.Request.Body.AsString());
      return 200;
    }


    // Requires a JSON in the body
    private dynamic connectDAQ(){

      log.info("Got request for DAQ connect");

      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        DAQconfig d = serializer.Deserialize<DAQconfig>(memer);
        log.info($"With parameters:");
        log.info($"samplerate:\t{d.samplerate}");
        log.info($"segment length:\t{d.segmentLength}");

        bool connect = controller.connectDAQ(d);

        if (connect){
          log.ok("DAQ connected");
          log.ok("initializing DSP");
          controller.initDSP();

          return 200;
        }
        log.err("Connecting to DAQ failed");
        return 500; // what if it's just a generic error? dunno lol use remmina...
      }
      catch (Exception e){ // should only catch deserialize error, dunno how xD
        log.err("malformed request");
        Console.WriteLine(e);
        return 500;
      }
    }


    private dynamic startDAQ(){
      log.info("Got request for DAQ start");

      if (controller.startServer())
        {
          log.ok("DAQ server started");
          return 200;
        }

      log.err("DAQ server failed to start");
      return 500;
    }


    private dynamic stopDAQ(){
      log.info("Got request to stop DAQ");

      if (controller.stopServer())
        {
          log.ok("DAQ stopped");
          return 200;
        }

      log.err("Unable to stop DAQ signalled by DAQ stop return value");
      return 500;
    }


    private dynamic setRegs(){
      log.info("Got request for setting DSP registers");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        RegSetRequest r = serializer.Deserialize<RegSetRequest>(memer);

        log.info($"Got register set request");

        log.info($"Setting registers:");
        log.info(r.ToString());

        // log.err($"WARNING: SET REGS IS CURRENTLY SET TO NO-OP");
        var hur = controller.setRegs(r);
      }
      catch (Exception e){
        log.err("set regs malformed request");
        Console.WriteLine(e);
        return 500;
      }
      log.ok("Registers have been set");
      return 200;
    }


    private dynamic readRegs(){
      log.info("Got request for reading DSP registers");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        RegReadRequest r = serializer.Deserialize<RegReadRequest>(memer);

        RegReadResponse resp = controller.readRegs(r);

        log.info($"Got register read request");

        log.info($"Reading registers:");
        log.info(r.ToString());

        log.info($"Returning values:");
        log.info(resp.ToString());
        string output = JsonConvert.SerializeObject(resp);
        var hurr = Encoding.UTF8.GetBytes(output);

        return new Response
        {
          ContentType = "application/json",
          Contents = s => s.Write(hurr, 0, hurr.Length)
        };
      }
      catch (Exception e){
        log.err("read regs malformed request");
        Console.WriteLine(e);
        return 500;
      }
      log.ok("Registers read successful");
      return 200;
    }

    private dynamic stimReq(){
      log.info("got stim req");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        StimReq r = serializer.Deserialize<StimReq>(memer);

        controller.stimReq(r);

        log.info($"Got stim req {r.periods[0]}");
        log.info($"Got stim req {r.periods[1]}");
        log.info($"Got stim req {r.periods[2]}");
        log.info($"Got stim req {r.periods[3]}");
        log.info($"");
        log.info($"");

      }
      catch (Exception e){
        log.err("read regs malformed request");
        Console.WriteLine(e);
        return 500;
      }
      log.ok("stim applied successful");
      return 200;
    }

    private dynamic simpleStimReq(){
      log.info("Got request for setting DSP stim");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        BasicStimReq r = serializer.Deserialize<BasicStimReq>(memer);

        controller.basicStimReq(r);

        log.info($"Got stim req with period {r.period}");

      }
      catch (Exception e){
        log.err("read regs malformed request");
        Console.WriteLine(e);
        return 500;
      }
      log.ok("stim applied successful");
      return 200;
    }


    private dynamic stimTest(){
      log.info("Got request for testing DSP stim");
      controller.basicStimReq(new BasicStimReq { period = 0x10000} );

      return 200;
    }


    private dynamic connectDSP(){
      controller.initDSP();
      return 200;
    }


    private dynamic stimDump(){
      log.info("Got request for dumping DSP registers");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        RegReadRequest r = serializer.Deserialize<RegReadRequest>(memer);

        RegReadResponse resp = controller.readRegsDirect(r);

        log.info($"Got register direct read request");

        log.info($"Reading registers:");
        log.info(r.ToString());

        log.info($"Returning values:");
        log.info(resp.ToString());
        string output = JsonConvert.SerializeObject(resp);
        var hurr = Encoding.UTF8.GetBytes(output);

        return new Response
        {
          ContentType = "application/json",
          Contents = s => s.Write(hurr, 0, hurr.Length)
        };
      }
      catch (Exception e){
        log.err("read regs malformed request");
        log.err($"{e}");
        return 500;
      }
      log.ok("Registers read successful");
      return 200;
    }
  }
}
