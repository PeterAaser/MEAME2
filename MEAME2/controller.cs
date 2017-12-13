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
    void initDSP();
    string getDevicesDescription();
    bool setRegs(RegSetRequest r);
    RegReadResponse readRegs(RegReadRequest r);
    RegReadResponse readRegsDirect(RegReadRequest r);
    void basicStimReq(BasicStimReq s);
    void stimReq(StimReq s);
    void readDSPlog();
    void resetDebug();
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
      if(devicePresent && !DAQconfigured){
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
      else{
        log.info("Tried to connect to already connected/configured device");
      }
      return devicePresent && this.DAQconfigured;
    }


    public void initDSP(){
      this.dsp.uploadMeameBinary();
    }

    public bool setRegs(RegSetRequest r){
      return this.dsp.writeRegRequest(r);
    }

    public RegReadResponse readRegs(RegReadRequest r){
      RegReadResponse resp = new RegReadResponse();
      resp.values = this.dsp.readRegRequest(r);
      resp.addresses = r.addresses;
      return resp;
    }

    public RegReadResponse readRegsDirect(RegReadRequest r){
      RegReadResponse resp = new RegReadResponse();
      resp.values = this.dsp.readRegDirect(r);
      resp.addresses = r.addresses;
      return resp;
    }


    public void basicStimReq(BasicStimReq s){
      dsp.basicStimTest(s.period);
    }

    public void readDSPlog(){
      dsp.readLog();
    }

    public void resetDebug(){
      dsp.resetDebug();
    }


    public void stimReq(StimReq s){
      dsp.stimReq(s);
    }
  }
}
