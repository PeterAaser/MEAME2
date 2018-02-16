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

      Get["/"] = _ => "hello this is MEAME.";
      Post["/logmsg"] = _ => logmsg();

      Post["/DAQ/connect"]       = _ => connectDAQ();
      Get["/DAQ/start"]          = _ => startDAQ();
      Get["/DAQ/stop"]           = _ => stopDAQ();

      Post["/DSP/connect"]       = _ => connectDSP();
      Post["/DSP/setreg"]        = _ => setRegs();
      Post["/DSP/readreg"]       = _ => readRegs();

      Post["/DSP/stimreq"]       = _ => stimReq();
      Post["/DSP/stimGroupReq"]  = _ => stimGroupReq();
      Post["/DSP/stimtest"]      = _ => stimTest();

      Post["/DSP/barf"]          = _ => readDSPlog();
      Post["/DSP/reset_debug"]   = _ => resetDebug();

      Post["/DSP/call"]          = _ => callDspFunc();
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


    /**
       Does nothing useful, should let us know if MEAME2 is online,
       if anyone is currently using it, and what the settings are.
     */
    private dynamic status(){
      return 200;
    }

    /**
       Writes a message to MEAME2 console
     */
    private dynamic logmsg(){
      log.info(this.Request.Body.AsString());
      return 200;
    }


    /**

     */
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
        else{
          log.err("Connecting to DAQ failed");
          return 500;
        }
      }
      catch (Exception e){ // should only catch deserialize error, dunno how xD
        log.err($"{e}");
        return 500;
      }
    }


    private dynamic startDAQ(){
      log.info("Got request for DAQ start");

      if (controller.startServer()){
        log.ok("DAQ server started");
        return 200;
      }

      log.err("DAQ server failed to start");
      return 500;
    }


    private dynamic stopDAQ(){
      log.err("stopDAQ IS NOT IMPLEMENTED");
      return 200;
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

        var hur = controller.setRegs(r);
      }
      catch (Exception e){
        log.err("set regs malformed request");
        log.err($"{e}");
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
      // log.info("got stim req");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        StimReq r = serializer.Deserialize<StimReq>(memer);

      }
      catch (Exception e){
        log.err("read regs malformed request");
        Console.WriteLine(e);
        return 500;
      }
      // log.ok("stim applied successful");
      return 200;
    }

    private dynamic stimGroupReq(){
      log.info("got stim req group");
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        StimGroupReq r = serializer.Deserialize<StimGroupReq>(memer);

        controller.stimGroupReq(r);

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

    private dynamic readDSPlog(){
      log.info("reading log");
      controller.readDSPlog();
      return 200;
    }

    private dynamic resetDebug(){
      log.info("resetting mailbox");
      controller.resetDebug();
      return 200;
    }

    private dynamic connectDSP(){
      controller.initDSP();
      return 200;
    }

    private dynamic callDspFunc(){
      try {
        string body = this.getJsonBody();
        StringReader memeReader = new StringReader(body);
        JsonTextReader memer = new JsonTextReader(memeReader);
        JsonSerializer serializer = new JsonSerializer();
        DspFuncCall r = serializer.Deserialize<DspFuncCall>(memer);

        controller.executeDSPfunc(r);

        log.info($"Executed dsp func call");

      }
      catch (Exception e){
        log.err("dsp func call malformed request");
        Console.WriteLine(e);
        return 500;
      }
      return 200;
    }
  }
}
