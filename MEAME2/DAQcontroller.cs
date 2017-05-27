using System;
using System.Collections.Generic;

using Mcs.Usb;

namespace MEAME2
{

  public class DAQ {

    public int samplerate { get; set; }
    public int channelBlockSize { get; set; }
    static int mChannelHandles { get; set; }
    static int hwchannels { get; set; }
    static int channelblocksize { get; set; }
    public SampleSizeNet dataFormat { get; set; }
    public Action<int, int[], int, int> onChannelData { get; set; }

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
      dataAcquisitionDevice.EnableDigitalIn(true, 0);
      dataAcquisitionDevice.EnableChecksum(true, 0);
      dataAcquisitionDevice.SetDataMode(DataModeEnumNet.dmSigned32bit, 0);

      int ana, digi, che, tim, block;
      dataAcquisitionDevice.GetChannelLayout(out ana, out digi, out che, out tim, out block, 0);

      dataAcquisitionDevice.SetSampleRate(samplerate, 1, 0);

      int gain = dataAcquisitionDevice.GetGain();

      List<CMcsUsbDacqNet.CHWInfo.CVoltageRangeInfoNet> voltageranges;
      dataAcquisitionDevice.HWInfo().
        GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(out voltageranges);

      bool[] selectedChannels = new bool[block];
      for (int i = 0; i < block; i++){ selectedChannels[i] = true; } // hurr

      channelblocksize = 128;

      dataAcquisitionDevice.SetSelectedData(selectedChannels,
                                            10 * channelblocksize,
                                            channelblocksize,
                                            dataFormat,
                                            block);

      mChannelHandles = block;

      dataAcquisitionDevice.ChannelBlock_SetCheckChecksum((uint)che, (uint)tim); // ???

      // int voltrange = voltageranges.ToArray()[0];

      int validDataBits = -1;
      int deviceDataFormat = -1;
      dataAcquisitionDevice.GetNumberOfDataBits(0,
                                                DacqGroupChannelEnumNet.HeadstageElectrodeGroup,
                                                out validDataBits);

      dataAcquisitionDevice.GetDataFormat(0,
                                          DacqGroupChannelEnumNet.HeadstageElectrodeGroup,
                                          out deviceDataFormat);

      DataModeEnumNet dataMode = dataAcquisitionDevice.GetDataMode(0);

      int meme = dataAcquisitionDevice.GetChannelsInBlock();

      deviceInfo =
        "Data acquisition device connected to physical device with parameters: \n" +
        $"number of blocks: {block}\n" +
        $"sample rate: {samplerate}\n" +
        $"Voltage range: {voltageranges[0].VoltageRangeDisplayStringMilliVolt}\n" +
        $"Corresponding to {voltageranges[0].VoltageRangeInMicroVolt} µV\n" +
        "--- channel layout ---\n" +
        $"hardware channels: {hwchannels}\n" +
        $"analog channels: {ana}\n" +
        $"digital channels: {digi}\n" +
        $"che(??) channels: {che}\n" +
        $"tim(??) channels: {tim}\n" +
        "---\n" +
        $"valid data bits: {validDataBits}\n" +          // 24
        $"device data format: {deviceDataFormat}\n" +    // 32
        $"device data mode: {dataMode}\n";               // dmSigned24bit

      return true;

    }

    private void _onChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples){

      int returnedFrames, totalChannels, offset, channels;

      dataAcquisitionDevice.ChannelBlock_GetChannel(0, 0, out totalChannels, out offset, out channels);

      int[] data = dataAcquisitionDevice.ChannelBlock_ReadFramesI32(0, channelblocksize, out returnedFrames);

      onChannelData(mChannelHandles, data, totalChannels, returnedFrames);
    }



    private void onError(String msg, int info){
      Console.WriteLine(info);
      Console.WriteLine(msg);
      Console.WriteLine("~~~~~~~~ Error! ~~~~~~~~~ :( ~~~~~~~~ )");
      dataAcquisitionDevice.StopDacq();
    }
  }
}
