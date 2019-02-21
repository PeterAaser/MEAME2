
using System;
using System.Net;
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
    public Action<int[],int> onChannelData { get; set; }
    public Action<int[][],int> onChannelDataFrame { get; set; }

    private int hasLogged = 0;

    public override String ToString(){
      return deviceInfo;
    }

    private readonly CMcsUsbListNet usblist = new CMcsUsbListNet();
    private CMeaDeviceNet dataAcquisitionDevice;
    private string deviceInfo = "Uninitialized DACQ device";

    // To say I hate writing code like this is an understatement
    public bool startDevice(){
      try { dataAcquisitionDevice.StartDacq(); return true; }
      catch (Exception e) {
	log.err($"{e}");
	return false;
      }
    }

    public bool stopDevice(){
      try { dataAcquisitionDevice.StopDacq(); return true; }
      catch (Exception e) {
	log.err($"{e}");
	return false;
      }
    }


    public bool connectDataAcquisitionDevice(uint index){

      try {

	// No idea what this is, got it from the example
	int virtualdevice = 0;

	// Divined from tea-leaves
	this.dataFormat = SampleSizeNet.SampleSize32Signed;

	var ls = usblist.GetUsbListEntry(index);
	log.info($"at index {index} getUsbListEntry is {usblist.GetUsbListEntry(index)}");
	log.info($"at index {index} getUsbListEntry Device ID is {usblist.GetUsbListEntry(index).DeviceId}");
	log.info($"at index {index} getUsbListEntry BusType is {usblist.GetUsbListEntry(index).DeviceId.BusType}");
	dataAcquisitionDevice = new CMeaDeviceNet(usblist.GetUsbListEntry(index).DeviceId.BusType,
						  _onChannelData,
						  onError);

	// The second arg refers to lock mask, allowing multiple device objects to be connected
	// to the same physical device. Yes, I know, what the fuck...
	dataAcquisitionDevice.Connect(usblist.GetUsbListEntry(index), 1);

	// This shouldn't be a problem, right?
	dataAcquisitionDevice.SendStop();

	// Get number of hw channels the device supports
	int what = 0;
	dataAcquisitionDevice.HWInfo().GetNumberOfHWADCChannels(out what);
	hwchannels = what;

	// None of this makes any sense to me
	dataAcquisitionDevice.SetNumberOfChannels(hwchannels);
	dataAcquisitionDevice.EnableDigitalIn(false, 0);
	dataAcquisitionDevice.EnableChecksum(false, 0);
	dataAcquisitionDevice.SetDataMode(DataModeEnumNet.dmSigned32bit, 0);

	// block:
	// get the number of 16 bit datawords which will be collected per sample frame,
	// use after the device is configured. (which means?, setting data mode, num channels etc?)
	int ana, digi, che, tim, block;
	dataAcquisitionDevice.GetChannelLayout(out ana, out digi, out che, out tim, out block, 0);

	log.info($"Setting samplerate to {samplerate}");
	uint oversample = 0;
	dataAcquisitionDevice.SetSampleRate(samplerate, oversample, virtualdevice);

	var deviceSampleRate = dataAcquisitionDevice.GetSampleRate(virtualdevice);
	log.info($"According to the device the samplerate should be {deviceSampleRate}");

	if(deviceSampleRate != samplerate){
	  log.err($"DAQ error. Requested samplerate: {samplerate}, registered samplerate: {deviceSampleRate}");
	  return false;
	}

	int gain = dataAcquisitionDevice.GetGain();

	List<CMcsUsbDacqNet.CHWInfo.CVoltageRangeInfoNet> voltageranges;
	dataAcquisitionDevice.HWInfo().
	  GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(out voltageranges);

	/**
	   Whatever this retarded shit is it's probably wrong in some way
	*/
	bool[] selectedChannels = new bool[block*2];
	for (int i = 0; i < block*2; i++){ selectedChannels[i] = true; }

	bool[] nChannels         = selectedChannels;
	int queueSize            = 640000;
	int threshold            = segmentLength;
	SampleSizeNet sampleSize = dataFormat;           // Signed32
	int ChannelsInBlock      = block;                // 64

	dataAcquisitionDevice.SetSelectedData
	  (nChannels,
	   queueSize,
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
	  $"device data mode:   \t{dataMode}\n" +         // dmSigned32bit
	  "";

	return true;
      }
      catch (Exception e){
	log.err($"connecting to daq failed with {e}");
	return false;
      }
    }


    private void _onChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples){
      try {

	int returnedFrames, totalChannels, offset, channels;

	int handle = 0;
	int channelEntry = 0;
	int frames = 0;

	// int[] buf = dataAcquisitionDevice.ChannelBlock_ReadFramesI32(handle,
	// 																														 segmentLength,
	// 																														 out returnedFrames);

	// This 'always' prints out whatever segment length is set to be
	// log.info($"returnedFrames: {returnedFrames}");

	// onChannelData(buf, returnedFrames);

	int[][] buf = dataAcquisitionDevice.ChannelBlock_ReadAsFrameArrayI32(handle,
									     segmentLength,
									     out returnedFrames);

	onChannelDataFrame(buf, returnedFrames);

								
	if(hasLogged == 0){
	  log.info($"{buf.Length()}");    // 1000
	  log.info($"{buf[0].Length()}"); // 256 ??? WHAT (same shit repeated 4 times)

	  // var asString = String.Join(", ", buf[0]);
	  // var asString1= String.Join(", ", buf[1]);
	  // var asString2= String.Join(", ", buf[2]);
	  // var asString3= String.Join(", ", buf[3]);
	  // log.info("## 1 ##");
	  // log.info(asString);
	  // log.info("## 2 ##\n\n");
	  // log.info(asString1);
	  // log.info("## 3 ##\n\n");
	  // log.info(asString2);
	  // log.info("## 4 ##\n\n");
	  // log.info(asString3);
	  // var stringBuf = new List<String>();
	  // for(int ii = 0; ii < buf[0].Length(); ii++){
	  // 		stringBuf.Add($"{buf[ii][0]}");
	  // }

	  // var asString2 = String.Join("\n", stringBuf);

	  // log.info(asString);
	  // log.info(asString2);
										
												
	  hasLogged = 1;
	}

      }
      catch (Exception e){
	Console.ForegroundColor = ConsoleColor.Red;
	log.err("DAQ ERROR", "DAQ ");
	log.err($"{e}");
	dataAcquisitionDevice.Disconnect();
	throw e;
      }
    }


    private void onError(String msg, int info){
      Console.ForegroundColor = ConsoleColor.Red;
      log.err("DAQ on error invoked :(", "DAQ ");
      log.err($"{info}");
      log.err($"{msg}");

      dataAcquisitionDevice.StopDacq();
      dataAcquisitionDevice.Dispose();
    }
  }
}
