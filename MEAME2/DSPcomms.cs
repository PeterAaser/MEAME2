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
				public bool flashed = false;
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
								log.err("DSP DEVICE NOT FOUND");
						}
				}

				public bool uploadMeameBinary(){

						string FirmwareFile;

						// YOLO :---DDDd
						FirmwareFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
						FirmwareFile += @"\..\..\..\..\FB_Example.bin";

						log.info($"Uploading MEAME binary at {FirmwareFile}");

						if(!System.IO.File.Exists(FirmwareFile)){
								throw new System.IO.FileNotFoundException("Binary file not found");
						}

						log.info($"Found binary at {FirmwareFile}");
						log.info("Uploading new binary...");
						dspDevice.LoadUserFirmware(FirmwareFile, dspPort);           // Code for uploading compiled binary

						log.ok("Binary uploaded, reconnecting device...");

						Thread.Sleep(100);

						//if we got this far it probably works...
						this.flashed = true;

						return true;
				}
		}
}
