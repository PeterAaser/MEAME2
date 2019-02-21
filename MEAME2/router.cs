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


      Post["/DAQ/connect"]       = _ => connectDAQ();
      Get["/DAQ/start"]          = _ => startDAQ();
      Get["/DAQ/stop"]           = _ => stopDAQ();

      Get["/status"]             = _ => status();

      Get["/DSP/flash"]          = _ => flashDsp();
      Post["/DSP/call"]          = _ => callDspFunc();
      Post["/DSP/read"]          = _ => readDspRegs();
      Post["/DSP/write"]         = _ => writeDspRegs();
      Get["/DSP/replay"]         = _ => replayDspRequests();

      Post["/aux/logmsg"]        = _ => logmsg();
    }


    private dynamic status(){
      string output = JsonConvert.SerializeObject(controller.getMEAMEstatus());
      return respondJsonOk(output);
    }

    private dynamic logmsg(){
      log.info(this.Request.Body.AsString());
      return 200;
    }

    private dynamic connectDAQ(){

      log.info("Got request for DAQ connect");

      DAQconfig d = decode<DAQconfig>(this.getJsonBody());

      log.info($"Connecting DAQ with params:");
      log.info($"samplerate:\t{d.samplerate}");
      log.info($"segment length:\t{d.segmentLength}");

      bool connect = controller.connectDAQ(d);

      if (connect){
        log.ok("DAQ connected");
        return 200;
      }
      else{
        log.err("Connecting to DAQ failed");
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

    private dynamic flashDsp(){
      // controller.flashDsp();
      return 200;
    }

    private dynamic callDspFunc(){
      log.info("got write req");
      DspFuncCall f = decode<DspFuncCall>(this.getJsonBody());

      if(controller.executeDspFunc(f))
        return 200;
      else
        return 500;
    }

    private dynamic readDspRegs(){
      log.info("got write req");
      RegReadRequest r = decode<RegReadRequest>(this.getJsonBody());
      var reads = controller.executeDspRead(r);

      string output = JsonConvert.SerializeObject(reads);
      return respondJsonOk(output);
    }

    private dynamic writeDspRegs(){
      log.info("got write req");
      string body = this.getJsonBody();
      RegWriteRequest r = decode<RegWriteRequest>(body);
      controller.executeDspWrite(r);

      return 200;
    }

    private dynamic replayDspRequests()
    {
      // (TODO): Hard code the replay to use for now. When different
      // replays are needed this will have to be fixed.
      string replay = "SHODANlog.json";
      DspInteraction[] dspInteractions =
        CommandSerializer.fromJSONFile<DspInteraction[]>(replay);

      foreach (DspInteraction dspCall in dspInteractions)
      {
        string dspCallType = dspCall.GetType().Name.ToString();
        switch (dspCallType)
        {
          case "DspFuncCall":
            controller.executeDspFunc((DspFuncCall) dspCall);
            break;
          case "RegWriteRequest":
            controller.executeDspWrite((RegWriteRequest) dspCall);
            break;
          default:
            // Not exactly a useful return code for the client, but at
            // least we print something helpful in MEAME.
            log.err("Error deserializing replay of DSP calls");
            log.err($"Got dspCallType {dspCallType}");
            return 500;
        }
      }

      return 200;
    }

    private string getJsonBody(){
      var id = this.Request.Body;
      long length = this.Request.Body.Length;
      byte[] data = new byte[length];
      id.Read(data, 0, (int)length);
      string body = System.Text.Encoding.Default.GetString(data);
      return body;
    }

    private T decode<T>(string body){
      StringReader memeReader = new StringReader(body);
      JsonTextReader memer = new JsonTextReader(memeReader);
      JsonSerializer serializer = new JsonSerializer();
      try {
        T r = serializer.Deserialize<T>(memer);
        if(r == null){
          log.err("deserialize error");
        }
        return r;
      }
      catch (Exception e){
        log.err("deserialize for string {body} threw exception {e}");
        throw e;
      }
    }


    private Response respondJsonOk(string resp){
      var raw = Encoding.UTF8.GetBytes(resp);
      return new Response
      {
        ContentType = "application/json",
        Contents = s => s.Write(raw, 0, raw.Length)
      };
    }
  }
}
