using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MEAME2
{
  public class MEAMEcontrol
  {
    private ConnectionManager cm = new ConnectionManager();
    private ChannelServer     channelServer = new ChannelServer();
    private DAQ               daq = new DAQ();
    private CMcsUsbListNet    usblist = new CMcsUsbListNet();
    private bool              DAQconfigured = false;
    private bool              DAQrunning = false;

    private String[] devices;

    public MEAMEcontrol(){
    }

    public string getDevicesDescription(){
      updateDeviceList();
      var message = new { Devices = devices };
      return JsonConvert.SerializeObject(message);
    }

    public void updateDeviceList(){
      usblist.Initialize(DeviceEnumNet.MCS_MEA_DEVICE);
      devices = new String[usblist.Count];
      for (uint ii = 0; ii < usblist.Count; ii++){
        devices[ii] =
          usblist.GetUsbListEntry(ii).DeviceName + " / "
          + usblist.GetUsbListEntry(ii).SerialNumber;
      }
    }


    public bool startServer(){
      if(this.DAQconfigured && !DAQrunning){
        this.daq.startDevice();
        DAQrunning = true;
        return true;
      }
      Console.WriteLine("startServer returns 500");
      return false;
    }


    public bool stopServer(){
      if(this.DAQconfigured && DAQrunning){
        this.daq.stopDevice();
        this.DAQrunning = false;
        return true;
      }
      Console.WriteLine("stopServer returns 500");
      return false;
    }


    public bool connectDAQ(DAQconfig d){
      updateDeviceList();
      bool devicePresent = (devices.Any(p => p[p.Length - 1] == 'A'));
      if(devicePresent){
        this.daq.samplerate = d.samplerate;
        this.daq.segmentLength = d.segmentLength;
        this.daq.onChannelData = d.cm.OnChannelData;
        this.DAQconfigured = this.daq.connectDataAcquisitionDevice(0); // YOLO index arg
      }
      return devicePresent && this.DAQconfigured;
    }
  }
}
