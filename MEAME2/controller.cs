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
    bool stopServer();
    bool connectDAQ(DAQconfig d);
    string getDevicesDescription();
    bool testDSP();
    bool setRegs(RegSetRequest r);
    RegReadResponse readRegs(RegReadRequest r);
    void basicStimReq(BasicStimReq s);
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


    public bool startServer(){

      var tmp = getDevicesDescription();
      try {
        if(this.DAQconfigured && !DAQrunning){
          this.daq.startDevice();
          DAQrunning = true;
          this.channelServer.startListener();
          return true;
        }
      }
      catch (Exception e) {
        log.err("startServer exception");
        Console.WriteLine(e);
        throw e;
        return false;
      }
      return false;
    }


    public bool stopServer(){
      try{
        if(this.DAQconfigured && DAQrunning){
          this.daq.stopDevice();
          this.DAQrunning = false;
          return true;
        }
      }
      catch (Exception e) {
        // uhh...
        throw e;
      }
      return false;
    }


    public bool connectDAQ(DAQconfig d){

      this.updateDeviceList();

      bool devicePresent = (devices.Any(p => p[p.Length - 1] == 'A'));
      if(devicePresent){
        try {
          this.daq.samplerate = d.samplerate;
          this.daq.segmentLength = d.segmentLength;
          this.daq.onChannelData = this.cm.OnChannelData;
          this.DAQconfigured = this.daq.connectDataAcquisitionDevice(0); // YOLO index arg
          this.DAQconfigured = true;

          log.info(this.daq.ToString());
        }
        catch (Exception e) {
          // uhh...
          throw e;
        }
      }
      return devicePresent && this.DAQconfigured;
    }


    public bool initDSP(){
      return this.dsp.uploadMeameBinary();
    }


    public bool testDSP(){
      // consoleInfo("Uploading MEAME binary");
      // initDSP();
      // return dsp.test();
      log.err("called method that does nothing");
      return true;
    }


    public bool setRegs(RegSetRequest r){
      return this.dsp.writeRegRequest(r);
    }
    public RegReadResponse readRegs(RegReadRequest r){
      RegReadResponse resp = new RegReadResponse();
      resp.values = this.dsp.readRegRequest(r);
      return resp;
    }


    public void basicStimReq(BasicStimReq s){
      this.dsp.basicStimTest(s.period);
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
