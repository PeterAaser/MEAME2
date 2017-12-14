/**
   This absolute horror is what I started with. Keep that in mind if you judge
   the abysmal code quality of MEAME2, it could've been even worse.
 */


// 9 programs for the price of one!
#define channelmethod
#define channeldata
//#define pollfordata

namespace MeaExampleNet
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    using Mcs.Usb;

    public partial class Form1 : Form
    {

        private readonly CMcsUsbListNet usblist = new CMcsUsbListNet();
        private CMeaDeviceNet device;


        // The overall number of channels (including data, digital, checksum, timestamp) in one sample. 
        // Checksum and timestamp are not available for MC_Card
        // With the MC_Card you lose one analog channel, when using the digital channel 
        int channelblocksize;
        int mChannelHandles;

        public Form1()
        {
            InitializeComponent();
        }

        private void BtMeaDevicePresentClick(object sender, EventArgs e)
        {
            cbDevices.Items.Clear();
            usblist.Initialize(DeviceEnumNet.MCS_MEA_DEVICE);
            for (uint i = 0; i < usblist.Count; i++)
            {
                cbDevices.Items.Add(usblist.GetUsbListEntry(i).DeviceName + " / " + usblist.GetUsbListEntry(i).SerialNumber);
            }
            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
            }

        }

        private void CbDevicesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (device != null)
            {
                device.StopDacq();

                device.Disconnect();
                device.Dispose();

                device = null;
            }

            uint sel = (uint)cbDevices.SelectedIndex;
            /* choose one of the following contructors:
             * The first one uses the OnNewData callback and gives you a reference to the raw multiplexed data,
             * this could be used without further initialisation
             * The second uses the more advanced callback which gives you the data for each channel in a callback, but need initialistation
             * for buffers and the selected channels
             */

            if (sel >= 0) // a device is selected, enable the sampling buttons
            {
                btStart.Enabled = true;
            }
            else
            {
                btStart.Enabled = false;
            }
            btStop.Enabled = false;

            device = new CMeaDeviceNet(usblist.GetUsbListEntry(sel).DeviceId.BusType, OnChannelData, OnError);

            device.Connect(usblist.GetUsbListEntry(sel));

            device.SendStop(); // only to be sure

            tbDeviceInfo.Text = "";
            tbDeviceInfo.Text += /*"Serialnumber: " +*/ device.SerialNumber + Environment.NewLine;
            int hwchannels;
            device.HWInfo().GetNumberOfHWADCChannels(out hwchannels);
            tbDeviceInfo.Text += @"Number of Hardwarechannels: " + hwchannels.ToString("D") + Environment.NewLine;

            if (hwchannels == 0)
            {
                hwchannels = 64;
            }

            
            // configure MeaDevice: MC_Card or Usb
            device.SetNumberOfChannels(hwchannels);

            const int Samplingrate = 20000; // MC_Card does not support all settings, please see MC_Rack for valid settings
            device.SetSampleRate(Samplingrate, 1, 0);
            
            int gain = device.GetGain();

            List<CMcsUsbDacqNet.CHWInfo.CVoltageRangeInfoNet> voltageranges;
            device.HWInfo().GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(out voltageranges);
            for (int i = 0; i < voltageranges.Count; i++)
            {
                tbDeviceInfo.Text += @"(" + i.ToString("D") + @") " + voltageranges[i].VoltageRangeDisplayStringMilliVolt + Environment.NewLine;
            }

            // Set the range acording to the index (only valid for MC_Card)
            // device.SetVoltageRangeInMicroVoltByIndex(0, 0);

            device.EnableDigitalIn(true, 0);
 
            // Checksum not supported by MC_Card
            device.EnableChecksum(true, 0);


            // Get the layout to know how the data look like that you receive
            int ana, digi, che, tim, block;
            device.GetChannelLayout(out ana, out digi, out che, out tim, out block, 0);

            // or
            block = device.GetChannelsInBlock();

            // set the channel combo box with the channels
            SetChannelCombo(block);

            channelblocksize = Samplingrate / 10; // good choise for MC_Card

            bool[] selChannels = new bool[block];

            for (int i = 0; i < block; i++)
            {
                selChannels[i] = true; // With true channel i is selected
                // selChannels[i] = false; // With false the channel i is deselected
            }
            // queue size and threshold should be selected carefully
#if channelmethod
#if channeldata
            device.SetSelectedData(selChannels, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned, block);
            //device.AddSelectedChannelsQueue(10, 2, 10 * channelblocksize, channelblocksize, SampleSizeNet.SampleSize16Unsigned);
            //device.ChannelBlock_SetCommonThreshold(channelblocksize);
            // Alternative call if you want to select all channels
            //device.SetSelectedData(block, 10 * channelblocksize, channelblocksize, CMcsUsbDacqNet.SampleSize.Size16, block);
            mChannelHandles = block; // for this case, if all channels are selected
#else // !channeldata
                device.SetSelectedChannels(selChannels, 10 * channelblocksize, channelblocksize, CMcsUsbDacqNet.SampleSize.Size16);
                m_channel_handles = block; // for this case, if all channels are selected
#endif // !channeldata
#else // !channelmethod
                device.SetSelectedChannelsQueue(selChannels, 10 * channelblocksize, channelblocksize, CMcsUsbDacqNet.SampleSize.Size16);
#endif // !channelmethod
            device.ChannelBlock_SetCheckChecksum((uint)che, (uint)tim);
        }

        /* Here follow the callback funktion for receiving data and receiving error messages
         * Please note, it is an error to use both data receiving callbacks at a time unless you know want you are doing
         */

#if channelmethod
        delegate void OnChannelDataDelegate(ushort[] data, int offset);
#else
        delegate void OnChannelDataDelegate(Dictionary<int, ushort[]> data);
#endif

#if pollfordata
        void  timer_Tick(object sender, EventArgs e)
        {
            int size_ret;
#if channelmethod
#if channeldata
            uint frames = device.ChannelBlock_AvailFrames(0);
            if (frames > channelblocksize)
            {
                int totalchannels, offset, channels;
                device.ChannelBlock_GetChannel(0, 0, out totalchannels, out offset, out channels);
                ushort[] data = device.ChannelBlock_ReadFramesUI16(0, channelblocksize, out size_ret);
                for (int i = 0; i < totalchannels; i++)
                {
                    ushort[] data1 = new ushort[channelblocksize];
                    for(int j = 0;j < channelblocksize;j++)
                    {
                        data1[j] = data[j * m_channel_handles + i];
                    }
                    OnChannelDataLater(data1, i);
                }
            }
#else // !channeldata
            for(int i = 0;i < m_channel_handles;i++)
            {
                uint frames = device.ChannelBlock_AvailFrames(i);
                if (frames > channelblocksize)
                {
                    int totalchannels, offset, channels;
                    device.ChannelBlock_GetChannel(i, 0, out totalchannels, out offset, out channels);
                    Debug.Assert(totalchannels == 1);
                    Debug.Assert(channels == 1);
                    ushort[] data = device.ChannelBlock_ReadFramesUI16(i, channelblocksize, out size_ret);
                    OnChannelDataLater(data, offset);
                }
            }
#endif // !channeldata
#else // !channelmethod
            uint frames = device.ChannelBlock_AvailFrames(0);
            if (frames > channelblocksize)
            {
                Dictionary<int, ushort[]> data = device.ChannelBlock_ReadFramesDictUI16(0, channelblocksize, out size_ret);
                OnChannelDataLater(data);
            }
#endif // !channelmethod
        }
#else // !pollfordata
        void OnChannelData(CMcsUsbDacqNet d, int cbHandle, int numSamples)
        {
            int sizeRet;
#if channelmethod
#if channeldata
            int totalchannels, offset, channels;
            device.ChannelBlock_GetChannel(0, 0, out totalchannels, out offset, out channels);
            ushort[] data = device.ChannelBlock_ReadFramesUI16(0, channelblocksize, out sizeRet);
            for (int i = 0; i < totalchannels; i++)
            {
                ushort[] data1 = new ushort[sizeRet];
                for (int j = 0; j < sizeRet; j++)
                {
                    data1[j] = data[j * mChannelHandles + i];
                }
                BeginInvoke(new OnChannelDataDelegate(OnChannelDataLater), new Object[] { data1, i });
            }
#else // !channeldata
            int totalchannels, offset, channels;
            device.ChannelBlock_GetChannel(CbHandle, 0, out totalchannels, out offset, out channels);
            Debug.Assert(totalchannels == 1);
            Debug.Assert(channels == 1);
            ushort[] data = device.ChannelBlock_ReadFramesUI16(CbHandle, numSamples, out size_ret);
            BeginInvoke(new OnChannelDataDelegate(OnChannelDataLater), new Object[] { data, offset });
#endif // !channeldata
#else // !channelmethod
            Dictionary<int, ushort[]> data = device.ChannelBlock_ReadFramesDictUI16(CbHandle, numSamples, out size_ret);
            BeginInvoke(new OnChannelDataDelegate(OnChannelDataLater), new Object[] { data });
#endif // !channelmethod
        }

        void OnError(String msg, int info)
        {
            device.StopDacq();
//            MessageBox.Show(@"Mea Device Error: " + msg);
        }

#endif // !pollfordata

#if channelmethod
       void OnChannelDataLater(ushort[] data, int offset)
        {
            int channel = cbChannel.SelectedIndex;
            if (channel >= 0 && channel == offset)
            {
                DrawChannel(data);
            }
        }
#else
        void OnChannelDataLater(Dictionary<int, ushort[]> data)
        {
            int channel = cbChannel.SelectedIndex;
            if (channel >= 0)
            {
                DrawChannel(data[channel]);
            }
        }
#endif

        private void BtStartClick(object sender, EventArgs e)
        {
            device.StartDacq();
            //device.StartDacq(150, 100, 300, 5);
            btMeaDevice_present.Enabled = false;
            cbDevices.Enabled = false;
            btStart.Enabled = false;
            btStop.Enabled = true;
#if pollfordata
            timer.Enabled = true;
#endif
        }

        private void BtStopClick(object sender, EventArgs e)
        {
            device.StopDacq();
            btMeaDevice_present.Enabled = true;
            cbDevices.Enabled = true;
            btStart.Enabled = true;
            btStop.Enabled = false;
#if pollfordata
            timer.Enabled = false;
#endif
        }

        private void SetChannelCombo(int channels)
        {
            cbChannel.Items.Clear();
            for (int i = 0; i < channels; i++)
            {
                cbChannel.Items.Add((i + 1).ToString("D"));
            }
            if (channels > 0)
            {
                cbChannel.SelectedIndex = 0;
            }
        }

        private void CbChannelSelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1FormClosed(object sender, FormClosedEventArgs e)
        {
            if (device != null)
            {
                device.SendStop();

                device.Disconnect();
                device.Dispose();

                device = null;
            }

        }

        ushort[] mData;

        void DrawChannel(ushort[] data)
        {
            mData = data;
            panel1.Invalidate();
        }

        private void Panel1Paint(object sender, PaintEventArgs e)
        {
            int width = panel1.Width;
            int height = panel1.Height;
            int max = 0;
            int min = 65536;
            if (mData != null && mData.Length > 1)
            {
                foreach (ushort t in mData)
                {
                    if (t > max)
                    {
                        max = t;
                    }
                    if (t < min)
                    {
                        min = t;
                    }
                }
                Point[] points = new Point[mData.Length];
                for (int i = 0; i < mData.Length; i++)
                {
                    points[i] = new Point(i * width / mData.Length, (mData[i]-min + 1) * height / (max-min + 2));
                }
                Pen pen = new Pen(Color.Black, 1);
                e.Graphics.DrawLines(pen, points);
            }
 
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device != null)
            {
                if (btMeaDevice_present.Enabled == false) // this means whe are sampling
                {
                    e.Cancel = true;
                }
            }
        }
    }

}
