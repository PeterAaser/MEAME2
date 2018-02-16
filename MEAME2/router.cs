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

      // Post["/DSP/barf"]          = _ => readDSPlog();

      Post["/DSP/call"]          = _ => callDspFunc();
      Post["/DSP/read"]          = _ => readDspRegs();
    }


    private dynamic status(){
      return 200;
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
        log.ok("initializing DSP");
        controller.initDsp();

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

    private dynamic readDspLog(){
      log.info("reading log");
      controller.readDspLog();
      return 200;
    }

    private dynamic connectDsp(){
      controller.initDsp();
      return 200;
    }

    private dynamic callDspFunc(){
      DspFuncCall f = decode<DspFuncCall>(this.getJsonBody());
      controller.executeDspFunc(f);
      log.info($"Executed dsp func call");

      return 200;
    }

    private dynamic readDspRegs(){
      RegReadRequest r = decode<RegReadRequest>(this.getJsonBody());
      controller.executeDspRead(r);

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
      T r = serializer.Deserialize<T>(memer);
      return r;
    }
  }
}
