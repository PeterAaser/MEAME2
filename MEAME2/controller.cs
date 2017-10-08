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
  }

  public class MEAMEcontrol : IMEAMEcontrol
  {
    private ConnectionManager cm;
    private ChannelServer     channelServer;
    private DAQ               daq;
    private CMcsUsbListNet    usblist;
    private bool              DAQconfigured;
    private bool              DAQrunning;

    private String[] devices;

    public MEAMEcontrol(){
      Console.WriteLine("Creating new controller");
      this.daq = new DAQ();
      this.cm = new ConnectionManager();
      this.cm.daq = this.daq;
      this.channelServer = new ChannelServer(cm);
      this.usblist = new CMcsUsbListNet();
      this.DAQconfigured = false;
      this.DAQrunning = false;
    }


    public string getDevicesDescription(){
      updateDeviceList();
      var message = new { Devices = devices };
      Console.WriteLine("----------- Device description is ----------------");
      Console.WriteLine(message);
      Console.WriteLine("-----------");
      return JsonConvert.SerializeObject(message);
    }

    private void updateDeviceList(){
      Console.WriteLine("Updating device list");
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
      Console.WriteLine("\nStarting server");
      Console.WriteLine($"DAQconfigured: {DAQconfigured}");
      Console.WriteLine($"DAQrunning: {DAQrunning}");
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
        Console.WriteLine("---[ERROR]----");
        Console.WriteLine("startServer threw");
        Console.WriteLine(e);
        Console.WriteLine("---[ERROR]----");
        throw e;
        return false;
      }
      Console.WriteLine("startServer returns 500");
      Console.WriteLine($"daq configured was: {this.DAQconfigured}");
      Console.WriteLine($"daq running was: {this.DAQrunning}");
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
        Console.WriteLine("---[ERROR]----");
        Console.WriteLine("stopServer threw");
        Console.WriteLine(e);
        Console.WriteLine("---[ERROR]----");
        throw e;
        return false;
      }
      Console.WriteLine("stopServer returns 500");
      Console.WriteLine($"daq configured was: {this.DAQconfigured}");
      Console.WriteLine($"daq running was: {this.DAQrunning}");
      return false;
    }


    public bool connectDAQ(DAQconfig d){
      Console.WriteLine("\nConnection DAQ");
      this.updateDeviceList();

      Console.WriteLine(this.devices);
      bool devicePresent = (devices.Any(p => p[p.Length - 1] == 'A'));
      if(devicePresent){
        try {
          this.daq.samplerate = d.samplerate;
          this.daq.segmentLength = d.segmentLength;
          this.daq.onChannelData = this.cm.OnChannelData;
          this.DAQconfigured = this.daq.connectDataAcquisitionDevice(0); // YOLO index arg
          Console.WriteLine($"DAQ connected {this.DAQconfigured}");
          this.DAQconfigured = true; // megaYOLO
          Console.WriteLine(this.daq);
          this.cm.channels = d.specialChannels;
        }
        catch (Exception e) {
          Console.WriteLine("---[ERROR]----");
          Console.WriteLine("connectDAQ threw");
          Console.WriteLine(e);
          Console.WriteLine("---[ERROR]----");
          throw e;
        }
      }
      return devicePresent && this.DAQconfigured;
    }
  }
}