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
  using Mcs.Usb;

  public partial class DSPComms {

    private CMcsUsbListNet usblist = new CMcsUsbListNet();
    private CMcsUsbListEntryNet dspPort;
    private CMcsUsbFactoryNet dspDevice;
    private uint requestID = 0;
    public bool connected = false;
    private uint lockMask = 64;
    public bool meameBinaryUploaded = false;

    public DSPComms()
    {
      dspDevice = new CMcsUsbFactoryNet();
      dspDevice.EnableExceptions(true);
      usblist.Initialize(DeviceEnumNet.MCS_MEAUSB_DEVICE); // Get list of MEA devices connect by USB

      bool dspPortFound = false;
      uint lockMask = 64;

      for (uint ii = 0; ii < usblist.Count; ii++){
        if (usblist.GetUsbListEntry(ii).SerialNumber.EndsWith("B")){
          dspPort = usblist.GetUsbListEntry(ii);
          dspPortFound = true;
          break;
        }
      }

      if(dspPortFound && (dspDevice.Connect(dspPort, lockMask) == 0)){
        connected = true;
        dspDevice.Disconnect();
      }
      else {
        Console.WriteLine("Fug!");
      }
    }


    public bool writeRegRequest(RegSetRequest regs){
      bool succ = true;
      for(int ii = 0; ii < regs.addresses.Length; ii++){
        succ = (succ && writeReg(regs.addresses[ii], regs.values[ii]));
      }
      // S U C C
      return succ;
    }

    public uint[] readRegRequest(RegReadRequest regs){
      uint[] results = new uint[regs.addresses.Length];
      for(int ii = 0; ii < regs.addresses.Length; ii++){
        results[ii] = readReg(regs.addresses[ii]);
      }
      return results;
    }



    // TODO: Does this actually perform a sufficient factory reset?
    // clearly no...
    public void resetDevices()
    {
      if(dspDevice.Connect(dspPort, lockMask) == 0)
        {
          Console.WriteLine("resetting MCU1");
          dspDevice.Coldstart(CFirmwareDestinationNet.MCU1);
        }
      else{
        Console.WriteLine("Connection Error when attempting to reset device");
        return;
      }

      dspDevice.Disconnect();
    }



    // The binary upload functions should probably be moved, possibly even to a different program.
    private void uploadBinary(String path)
    {

      if(!System.IO.File.Exists(path)){
        throw new System.IO.FileNotFoundException("Binary file not found");
      }

      consoleInfo($"Found binary at {path}");
      consoleInfo("Uploading new binary...");
      dspDevice.LoadUserFirmware(path, dspPort);           // Code for uploading compiled binary

      consoleOK("Binary uploaded, reconnecting device...");
    }


    public bool uploadMeameBinary()
    {
      string FirmwareFile;

      // YOLO :---DDDd
      FirmwareFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      FirmwareFile += @"\..\..\..\..\FB_Example.bin";

      consoleInfo($"Uploading MEAME binary at {FirmwareFile}");
      uploadBinary(FirmwareFile);
      this.meameBinaryUploaded = true;

      Thread.Sleep(400);

      return this.pingTest();
    }


    public void uploadOldBinary()
    {
      string FirmwareFile;

      // YOLO :---DDDd
      FirmwareFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      FirmwareFile += @"\..\..\..\..\control_group.bin";

      Console.WriteLine("Uploading control binary");
      uploadBinary(FirmwareFile);
    }

    // public void triggerOldStimReq()
    // {
    //   if(dspDevice.Connect(dspPort, lockMask) == 0)
    //     {
    //       b *= 2;
    //       if(b > 1000000){ b = 10000; }

    //       dspDevice.WriteRegister(DAC_ID, b);
    //     }
    //   else{ Console.WriteLine("Connection Error"); return; }
    //   dspDevice.Disconnect();
    // }

  }
}
