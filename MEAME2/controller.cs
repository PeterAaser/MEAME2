using System;
using Mcs.Usb;
using System.Net;
using System.Linq;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MEAME2
{
  public interface IMEAMEcontrol
  {
    bool startServer();
    bool connectDAQ(DAQconfig d);
    string getDevicesDescription();
    void flashDsp();
    bool executeDspFunc(DspFuncCall c);
    RegReadResponse executeDspRead(RegReadRequest r);
    void executeDspWrite(RegWriteRequest r);
    MEAMEstatus getMEAMEstatus();

  }

  public class MEAMEcontrol : IMEAMEcontrol
  {
    private ConnectionManager cm;
    private ChannelServer     channelServer;
    private DAQ               daq;
    private CMcsUsbListNet    usblist;
    private DSPComms          dsp;
    private bool              DAQconfigured;
    private bool              DAQrunning;
    private bool              dspConfigured = false;
    private Executor          dspExecutor;

    private String[] devices;

    public MEAMEcontrol(){
      this.daq = new DAQ();
      this.cm = new ConnectionManager();
      this.cm.daq = this.daq;
      this.channelServer = new ChannelServer(cm);
      this.usblist = new CMcsUsbListNet();
      this.DAQconfigured = false;
      this.DAQrunning = false;
      this.dsp = new DSPComms();

      this.dspExecutor = new MockExecutor();
      // this.dspExecutor = new LiveExecutor();
    }


    public string getDevicesDescription(){
      updateDeviceList();
      var message = new { Devices = devices };
      return JsonConvert.SerializeObject(message);
    }

    private void updateDeviceList(){
      usblist.Initialize(DeviceEnumNet.MCS_MEA_DEVICE);
      devices = new String[usblist.Count];
      for (uint ii = 0; ii < usblist.Count; ii++){
        devices[ii] =
          usblist.GetUsbListEntry(ii).DeviceName + " / "
          + usblist.GetUsbListEntry(ii).SerialNumber;
      }
      this.devices = devices;
    }


    public MEAMEstatus getMEAMEstatus(){
      return new MEAMEstatus{
        isAlive = this.DAQrunning,
        dspAlive = this.dsp.connected,
      };
    }


    public bool startServer(){
      try {
        if(this.DAQconfigured && !DAQrunning){
          this.daq.startDevice();
          DAQrunning = true;
          this.channelServer.startListener();
          return true;
        }
        else {
          log.info("Got start server req on already started server");
          return true;
        }
      }
      catch (Exception e) {
        log.err("startServer exception");
        log.err($"{e}");
        return false;
      }
    }


    public bool connectDAQ(DAQconfig d){

      this.updateDeviceList();
      bool devicePresent = (devices.Any(p => p[p.Length - 1] == 'A'));
      log.info(String.Join("\n",devices));

      if(devicePresent && !DAQconfigured){
        try {
					this.DAQconfigured = true;
          this.daq.samplerate = d.samplerate;
          this.daq.segmentLength = d.segmentLength;
          this.daq.onChannelData = this.cm.OnChannelData;
          log.info("Attempting to connect to daq");
          this.DAQconfigured = this.daq.connectDataAcquisitionDevice(0); // YOLO index arg
          this.DAQconfigured = true;

          log.info("Seems like it worked");
          log.info(this.daq.ToString());
        }
        catch (Exception e) {
	    
          log.err($"{e}");
          // uhh...
          throw e;
        }
      }
      else{
        log.info("Tried to connect to already connected/configured device");
      }
      return devicePresent && this.DAQconfigured;
    }


    public void flashDsp(){
      this.dsp.uploadMeameBinary();
    }

    public bool executeDspFunc(DspFuncCall c){
      DspOp<bool> exec = new CallDspFunc(c.func, c.argAddrs, c.argVals);
      return this.dspExecutor.execute(exec);
    }

    public RegReadResponse executeDspRead(RegReadRequest r){
      DspOp<uint[]> exec = new ReadOp(r.addresses);
      uint[] reads = this.dspExecutor.execute(exec);
      return new RegReadResponse{
        addresses = r.addresses,
        values = reads
      };
    }

    public void executeDspWrite(RegWriteRequest r){
      DspOp<uint> exec = new WriteArgsOp(r.addresses, r.values);
      this.dspExecutor.execute(exec);
    }
  }
}
