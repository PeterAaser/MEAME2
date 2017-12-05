using System;
using System.Collections.Generic;

using Mcs.Usb;

namespace MEAME2
{

  public class DAQ {

    public int samplerate { get; set; }
    public int segmentLength { get; set; }
    static int mChannelHandles { get; set; }
    static int hwchannels { get; set; }
    public SampleSizeNet dataFormat { get; set; }
    public Action<Dictionary<int, int[]>, int> onChannelData { get; set; }

    private int barfCounter = 0;

    public override String ToString(){
      return deviceInfo;
    }

    private readonly CMcsUsbListNet usblist = new CMcsUsbListNet();
    private CMeaDeviceNet dataAcquisitionDevice;
    private string deviceInfo = "Uninitialized DACQ device";

    // To say I hate writing code like this is an understatement
    public bool startDevice(){
      try { dataAcquisitionDevice.StartDacq(); return true; }
      catch (Exception e) { return false; }
    }

    public bool stopDevice(){
      try { dataAcquisitionDevice.StopDacq(); return true; }
      catch (Exception e) { return false; }
    }


    public bool connectDataAcquisitionDevice(uint index){

      this.dataFormat = SampleSizeNet.SampleSize32Signed;

      if(dataAcquisitionDevice != null){
        dataAcquisitionDevice.StopDacq();
        dataAcquisitionDevice.Disconnect();
        dataAcquisitionDevice.Dispose();
        dataAcquisitionDevice = null;
        throw new System.ArgumentException("Reached bad code path", "DAQ is null, mcs cruft");
      }

      dataAcquisitionDevice = new CMeaDeviceNet(usblist.GetUsbListEntry(index).DeviceId.BusType,
                                                _onChannelData,
                                                onError);


      // The second arg refers to lock mask, allowing multiple device objects to be connected
      // to the same physical device. Yes, I know, what the fuck...
      dataAcquisitionDevice.Connect(usblist.GetUsbListEntry(index), 1);
      dataAcquisitionDevice.SendStop();

      int what = 0;
      dataAcquisitionDevice.HWInfo().GetNumberOfHWADCChannels(out what);
      hwchannels = what;

      dataAcquisitionDevice.SetNumberOfChannels(hwchannels);
      dataAcquisitionDevice.EnableDigitalIn(false, 0);
      dataAcquisitionDevice.EnableChecksum(false, 0);
      dataAcquisitionDevice.SetDataMode(DataModeEnumNet.dmSigned32bit, 0);

      // block:
      // get the number of 16 bit datawords which will be collected per sample frame,
      // use after the device is configured. (which means?, setting data mode, num channels etc?)
      int ana, digi, che, tim, block;
      dataAcquisitionDevice.GetChannelLayout(out ana, out digi, out che, out tim, out block, 0);

      dataAcquisitionDevice.SetSampleRate(samplerate, 1, 0);

      int gain = dataAcquisitionDevice.GetGain();

      List<CMcsUsbDacqNet.CHWInfo.CVoltageRangeInfoNet> voltageranges;
      dataAcquisitionDevice.HWInfo().
        GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(out voltageranges);


      bool[] selectedChannels = new bool[block/2];
      for (int i = 0; i < block/2; i++){ selectedChannels[i] = true; } // hurr


      // *org [[file:killme.cs::/%20a%20device%20are%20read%20in%2032%20bit%20data%20format%20nChannels%20=%20ChannelsInBlock/2][documentation]]
      bool[] nChannels         = selectedChannels;
      int queueSize            = 120000;
      int threshold            = segmentLength;
      SampleSizeNet sampleSize = dataFormat;
      int ChannelsInBlock      = block/2;

      dataAcquisitionDevice.SetSelectedChannelsQueue
        (nChannels,
         queueSize, // huh?
         threshold,
         sampleSize,
         ChannelsInBlock);

      mChannelHandles = block;

      dataAcquisitionDevice.ChannelBlock_SetCheckChecksum((uint)che, (uint)tim); // ???

      // int voltrange = voltageranges.ToArray()[0];

      int validDataBits = -1;
      int deviceDataFormat = -1;

      /**
      Summary:
          Get the real number of data bits.

      Remarks:
          This value may be different from the value returned by GetDataFormat, e.g. in
          MC_Card the data are shifted 2 bits so the real number is 14 while the data format
          is 16 bits
      */
      dataAcquisitionDevice.GetNumberOfDataBits(0,
                                                DacqGroupChannelEnumNet.HeadstageElectrodeGroup,
                                                out validDataBits);

      dataAcquisitionDevice.GetDataFormat(0,
                                          DacqGroupChannelEnumNet.HeadstageElectrodeGroup,
                                          out deviceDataFormat);

      DataModeEnumNet dataMode = dataAcquisitionDevice.GetDataMode(0);


      /**
      Summary:
         Get the number of 16 bit datawords which will be collected per sample frame,
         use after the device is configured.

      Returns:
         Number of 16 bit datawords per sample frame.
         Returns 132 (66 32 bit words???)
      */
      int meme = dataAcquisitionDevice.GetChannelsInBlock();

      deviceInfo =
        "Data acquisition device connected to physical device with parameters: \n" +
        $"[SetSelectedChannelsQueue arguments:]\n" +
        $"nChannels           \t{selectedChannels}\n" +
        $"queueSize:          \t{queueSize}\n" +
        $"threshold:          \t{threshold}\n" +
        $"samplesize:         \t{sampleSize}\n" +
        $"channelsInBlock:    \t{ChannelsInBlock}\n\n" +
        $"[Experiment params]\n" +
        $"sample rate:        \t{samplerate}\n" +
        $"Voltage range:      \t{voltageranges[0].VoltageRangeDisplayStringMilliVolt}\n" +
        $"Corresponding to    \t{voltageranges[0].VoltageRangeInMicroVolt} µV\n" +
        $"[Device channel layout]\n\n" +
        $"hardware channels:  \t{hwchannels}\n" +       // 64
        $"analog channels:    \t{ana}\n" +              // 128
        $"digital channels:   \t{digi}\n" +             // 2
        $"che(??) channels:   \t{che}\n" +              // 4
        $"tim(??) channels:   \t{tim}\n\n" +
        $"[Other..]\n" +
        $"valid data bits:    \t{validDataBits}\n" +    // 24
        $"device data format: \t{deviceDataFormat}\n" + // 32
        $"device data mode:   \t{dataMode}\n" +         // dmSigned24bit
        $"nice meme:          \t{meme}\n" +
        "";

      return true;
    }


    private void _onChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples){
      try {

        int returnedFrames, totalChannels, offset, channels;

        int handle = 0;
        int channelEntry = 0;
        int frames = 0;

        dataAcquisitionDevice.ChannelBlock_GetChannel
          (handle,
           channelEntry,
           out totalChannels,
           out offset,
           out channels);

        Dictionary<int,int[]> data = dataAcquisitionDevice.ChannelBlock_ReadFramesDictI32
          (handle,
           segmentLength,
           out returnedFrames);

        if(barfCounter < 3000){
          if(barfCounter == 2999){
            log.info($"totalChannels:  {totalChannels}", "DAQ DEBUG");
            log.info($"offset:         {totalChannels}", "DAQ DEBUG");
            log.info($"channels:       {totalChannels}", "DAQ DEBUG");
            log.info($"returnedFrames: {totalChannels}", "DAQ DEBUG");
          }
          barfCounter++;
        }

        onChannelData(data, returnedFrames);
      }
      catch (Exception e){
        Console.ForegroundColor = ConsoleColor.Red;
        log.err("DAQ ERROR", "DAQ ");
        Console.WriteLine(e);
        dataAcquisitionDevice.Disconnect();
        throw e;
      }
    }


    private void onError(String msg, int info){
      Console.ForegroundColor = ConsoleColor.Red;
      log.err("DAQ on error invoked :(", "DAQ ");
      // Console.WriteLine(info);
      // Console.WriteLine(msg);

      dataAcquisitionDevice.StopDacq();
      dataAcquisitionDevice.Dispose();
    }
  }
}
