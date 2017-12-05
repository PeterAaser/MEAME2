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


    public uint[] readRegDirect(RegReadRequest regs){
      uint[] results = new uint[regs.addresses.Length];
      if(connect()){
        for(int ii = 0; ii < regs.addresses.Length; ii++){
          results[ii] = dspDevice.ReadRegister(regs.addresses[ii]);
        }
      }
      disconnect();
      log.info("reg direct successfully disconnected");
      return results;
    }

    public bool stimRequest(BasicStimReq req){

      basicStimTest(req.period);

      return true;
    }


    // TODO: Does this actually perform a sufficient factory reset?
    // clearly no...
    public void resetDevices()
    {
      if(connect())
        {
          Console.WriteLine("resetting MCU1");
          dspDevice.Coldstart(CFirmwareDestinationNet.MCU1);
        }
      else{
        Console.WriteLine("Connection Error when attempting to reset device");
        return;
      }

      disconnect();
    }



    // The binary upload functions should probably be moved, possibly even to a different program.
    private void uploadBinary(String path)
    {

      if(!System.IO.File.Exists(path)){
        throw new System.IO.FileNotFoundException("Binary file not found");
      }

      log.info($"Found binary at {path}");
      log.info("Uploading new binary...");
      dspDevice.LoadUserFirmware(path, dspPort);           // Code for uploading compiled binary

      log.ok("Binary uploaded, reconnecting device...");
    }


    public bool uploadMeameBinary()
    {
      string FirmwareFile;

      // YOLO :---DDDd
      FirmwareFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      FirmwareFile += @"\..\..\..\..\FB_Example.bin";

      log.info($"Uploading MEAME binary at {FirmwareFile}");
      uploadBinary(FirmwareFile);

      Thread.Sleep(100);

      return true;
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
  }
}
