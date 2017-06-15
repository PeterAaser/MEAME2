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

    public override String ToString(){
      return deviceInfo;
    }

    private readonly CMcsUsbListNet usblist = new CMcsUsbListNet();
    private CMeaDeviceNet dataAcquisitionDevice;
    private string deviceInfo = "Uninitialized DACQ device";

    public bool startDevice(){
      Console.WriteLine("Starting device");
      dataAcquisitionDevice.StartDacq();
      Console.WriteLine("Device started");
      return true;
    }

    public bool stopDevice(){
      dataAcquisitionDevice.StopDacq();
      return true;
    }

    public bool connectDataAcquisitionDevice(uint index){

      Console.WriteLine("Connecting data acquisition object to device");
      Console.WriteLine($"samplerate: {samplerate}");
      Console.WriteLine($"segmentLength: {segmentLength}");

      this.dataFormat = SampleSizeNet.SampleSize32Signed;

      if(dataAcquisitionDevice != null){
        dataAcquisitionDevice.StopDacq();
        dataAcquisitionDevice.Disconnect();
        dataAcquisitionDevice.Dispose();
        dataAcquisitionDevice = null;
        Console.WriteLine("this shouldn't be printed 123");
      }

      Console.WriteLine("Creating data acquisition device");
      dataAcquisitionDevice = new CMeaDeviceNet(usblist.GetUsbListEntry(index).DeviceId.BusType,
                                                _onChannelData,
                                                onError);

      // The second arg refers to lock mask, allowing multiple device objects to be connected
      // to the same physical device. Yes, I know, what the fuck...
      Console.WriteLine("Connecting");
      dataAcquisitionDevice.Connect(usblist.GetUsbListEntry(index), 1);

      Console.WriteLine("Sending stop signal");
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


      bool[] selectedChannels = new bool[block];
      for (int i = 0; i < block; i++){ selectedChannels[i] = true; } // hurr

      // dataAcquisitionDevice.SetSelectedData(selectedChannels,
      //                                       10 * segmentLength,
      //                                       segmentLength,
      //                                       dataFormat,
      //                                       block);

      // dataAcquisitionDevice.AddSelectedChannelsQueue(
      //                                                0,
      //                                                1,
      //                                                10*segmentLength,
      //                                                segmentLength,
      //                                                dataFormat);

      dataAcquisitionDevice.SetSelectedChannelsQueue(
                               selectedChannels,
                               10*segmentLength,
                               segmentLength,
                               dataFormat,
                               block/2);

      mChannelHandles = block;

      dataAcquisitionDevice.ChannelBlock_SetCheckChecksum((uint)che, (uint)tim); // ???

      // int voltrange = voltageranges.ToArray()[0];

      int validDataBits = -1;
      int deviceDataFormat = -1;

      //
      // Summary:
      //     Get the real number of data bits.
      //
      // Remarks:
      //     This value may be different from the value returned by GetDataFormat, e.g. in
      //     MC_Card the data are shifted 2 bits so the real number is 14 while the data format
      //     is 16 bits
      dataAcquisitionDevice.GetNumberOfDataBits(0,
                                                DacqGroupChannelEnumNet.HeadstageElectrodeGroup,
                                                out validDataBits);

      dataAcquisitionDevice.GetDataFormat(0,
                                          DacqGroupChannelEnumNet.HeadstageElectrodeGroup,
                                          out deviceDataFormat);

      DataModeEnumNet dataMode = dataAcquisitionDevice.GetDataMode(0);

      //
      // Summary:
      //     Get the number of 16 bit datawords which will be collected per sample frame,
      //     use after the device is configured.
      //
      // Returns:
      //     Number of 16 bit datawords per sample frame.

      // Returns 132 (66 32 bit words???)
      int meme = dataAcquisitionDevice.GetChannelsInBlock();

      deviceInfo =
        "Data acquisition device connected to physical device with parameters: \n" +
        $"number of blocks: {block}\n" +
        $"sample rate: {samplerate}\n" +
        $"Voltage range: {voltageranges[0].VoltageRangeDisplayStringMilliVolt}\n" +
        $"Corresponding to {voltageranges[0].VoltageRangeInMicroVolt} µV\n" +
        "--- channel layout ---\n" +
        $"hardware channels: {hwchannels}\n" +           // 64
        $"analog channels: {ana}\n" +                    // 128
        $"digital channels: {digi}\n" +                  // 2
        $"che(??) channels: {che}\n" +                   // 4
        $"tim(??) channels: {tim}\n" +
        "---\n" +
        $"valid data bits: {validDataBits}\n" +          // 24
        $"device data format: {deviceDataFormat}\n" +    // 32
        $"device data mode: {dataMode}\n" +              // dmSigned24bit
        $"nice meme: {meme}\n" +
        "";

      return true;

    }

    private void _onChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples){
      Console.WriteLine("got data:");
      Console.WriteLine(d);
      Console.WriteLine(cbHandle);
      Console.WriteLine(numSamples);
      Console.WriteLine("\n\n\n");
      try {

        int returnedFrames, totalChannels, offset, channels;

        int handle = 0;
        int channelEntry = 0;
        int frames = 0;

        dataAcquisitionDevice.ChannelBlock_GetChannel(handle,
                                                      channelEntry,
                                                      out totalChannels,
                                                      out offset,
                                                      out channels);



        // int[] data = dataAcquisitionDevice.ChannelBlock_ReadFramesI32(
        //                                                               handle,
        //                                                               segmentLength,
        //                                                               out returnedFrames);

        Dictionary<int,int[]> data = dataAcquisitionDevice.ChannelBlock_ReadFramesDictI32(handle,
                                                                                          segmentLength,
                                                                                          out returnedFrames);

        onChannelData(data, returnedFrames);
      }
      catch (Exception e){
        Console.WriteLine("_onChannelData exception ---------------- ");
        Console.WriteLine(e);
        Console.WriteLine("_onChannelData exception ---------------- ");
      }
    }



    private void onError(String msg, int info){
      Console.WriteLine(info);
      Console.WriteLine(msg);
      Console.WriteLine("~~~~~~~~ Error! ~~~~~~~~~ :( ~~~~~~~~ )");
      try {
        dataAcquisitionDevice.StopDacq();
        dataAcquisitionDevice.Dispose();
      }
      catch (Exception e)
        {
          Console.WriteLine("Got exception");
          Console.WriteLine(e);
          Console.WriteLine("Ignoring.. YOLO");
        }
    }
  }
}


