{
    public class CMcsUsbDacqNet : CMcsUsbNet
    {
        protected CHWInfo m_hwinfo;

        //
        // Summary:
        //     The sampling frequency of the device in Hz.
        public virtual int SampleRate { get; set; }

        public event OnChannelData ChannelDataEvent;
        public event OnError ErrorEvent;

        // α
        // Summary:
        //     Adds a common FIFO queue for all channels. Data in callback will be a list per
        //     channel. Use ChannelBlock_ReadFramesDict... with handle = 0 to read the data.
        //
        // Parameters:
        //   nOffset:
        //     Number of channel to start with (counted in samplesize bytes).
        //
        //   nChannels:
        //     Number of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        // Returns:
        //     The handle to the Queue.
        //
        // Remarks:
        //     When using 32 bit data format, ChannelsInBlock is still the number of 16 bit
        //     channels per frame, as obtained from GetChannelsInBlock, while nChannels is the
        //     number of 32 bit channels to be read from the device. So when all channels from
        //     a device are read in 32 bit data format nChannels = ChannelsInBlock/2
        public virtual int AddSelectedChannelsQueue(int nOffset, int nChannels, int queuesize, int threshold, SampleSizeNet samplesize);


        // Summary:
        //     Adds a common FIFO queue for all channels. Data in callback will be a list per
        //     channel. Use ChannelBlock_ReadFramesDict... with handle = 0 to read the data.
        //
        // Parameters:
        //   nOffset:
        //     Number of channel to start with (counted in samplesize bytes).
        //
        //   selectedChannels:
        //     List of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        // Returns:
        //     The handle to the Queue.
        //
        // Remarks:
        //     When using 32 bit data format, ChannelsInBlock is still the number of 16 bit
        //     channels per frame, as obtained from GetChannelsInBlock, while nChannels is the
        //     number of 32 bit channels to be read from the device. So when all channels from
        //     a device are read in 32 bit data format nChannels = ChannelsInBlock/2
        public virtual int AddSelectedChannelsQueue(int nOffset, bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize);
        public virtual int AddSelectedChannelsQueue(int nOffset, bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize);
        public virtual int AddSelectedChannelsQueue(int nOffset, int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize);
        //
        // Summary:
        //     Get the number of sample frames already available in the FIFO.
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        // Returns:
        //     Number of sample frames available in the FIFO.
        public virtual uint ChannelBlock_AvailFrames(int handle);
        public virtual uint ChannelBlock_AvailFrames(int handle, int queue);
        public virtual uint ChannelBlock_GetChannel(int handle, int channelentry, out int totalchannels, out int offset, out int channels);
        public virtual uint ChannelBlock_GetCheckChecksum();
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of int16_t frame arrays.
        public virtual short[][] ChannelBlock_ReadAsFrameArrayI16(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   queue:
        //     Number of the sub queue.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of int16_t frame arrays.
        public virtual short[][] ChannelBlock_ReadAsFrameArrayI16(int handle, int queue, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of int32_t frame arrays.
        public virtual int[][] ChannelBlock_ReadAsFrameArrayI32(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   queue:
        //     Number of the sub queue.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of int32_t frame arrays.
        public virtual int[][] ChannelBlock_ReadAsFrameArrayI32(int handle, int queue, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of uint16_t frame arrays.
        public virtual ushort[][] ChannelBlock_ReadAsFrameArrayUI16(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   queue:
        //     Number of the sub queue.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of uint16_t frame arrays.
        public virtual ushort[][] ChannelBlock_ReadAsFrameArrayUI16(int handle, int queue, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of uint32_t frame arrays.
        public virtual uint[][] ChannelBlock_ReadAsFrameArrayUI32(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue as array of uint16_t data frame arrays
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedData call was used.
        //
        //   queue:
        //     Number of the sub queue.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of uint32_t frame arrays.
        public virtual uint[][] ChannelBlock_ReadAsFrameArrayUI32(int handle, int queue, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in int16_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedChannelsQueue call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of int16_t arrays and hardware channel as key.
        public virtual Dictionary<int, short[]> ChannelBlock_ReadFramesDictI16(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in int32_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedChannelsQueue call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of int32_t arrays and hardware channel as key.
        public virtual Dictionary<int, int[]> ChannelBlock_ReadFramesDictI32(int handle, int frames, out int frames_ret);
        // β
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Zero when the SetSelectedChannelsQueue call was used.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of uint32_t arrays and hardware channel as key.
        public virtual Dictionary<int, uint[]> ChannelBlock_ReadFramesDictUI32(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        public virtual int[] ChannelBlock_ReadFramesI32(int handle, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   buffer:
        //     Buffer to put the data from the device in.
        //
        //   frames_pos:
        //     Position in buffer where to put the data.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Error Status. 0 on success.
        public virtual uint ChannelBlock_ReadFramesI32(int handle, int[] buffer, int frames_pos, int frames, out int frames_ret);

        //
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   buffer:
        //     Buffer to put the data from the device in.
        //
        //   frames_pos:
        //     Position in buffer where to put the data.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Error Status. 0 on success.
        public virtual uint ChannelBlock_ReadFramesUI32(int handle, uint[] buffer, int frames_pos, int frames, out int frames_ret);
        public virtual void ChannelBlock_SetCheckChecksum(uint checksumchannels, uint checksumoffset_from_end);
        public virtual void ChannelBlock_SetCommonThreshold(int common_threshold);
        //
        // Summary:
        //     Gets the adapter which is connected to the MEA2100 device.
        //
        // Returns:
        //     AdapterTypeEnumNet which enumerates the possible adapters.
        public virtual AdapterTypeEnumNet GetAdapterType();
        //
        // Summary:
        //     Gets the ADC data format, 16 means 16 bits, 24 means 24 bits, 32 means 32 bits.
        //
        // Returns:
        //     The data format in bits.
        public virtual uint GetAdcDataFormat();
        public virtual uint GetAdcZero(uint virtualDevice, DacqGroupChannelEnumNet group, out int adcz);
        public virtual uint GetAnalogValueUnit(uint virtualDevice, DacqGroupChannelEnumNet group, out AnalogUnitEnumNet unit);
        public int GetChannelDataFillSize();
        public virtual uint GetChannelLayout(out int AnalogChannels, out int DigitalChannels, out int ChecksumChannels, out int TimestampChannels, out int ChannelsInBlock, uint VirtualDevice);
        //
        // Summary:
        //     Get the number of 16 bit datawords which will be collected per sample frame,
        //     use after the device is configured.
        //
        // Returns:
        //     Number of 16 bit datawords per sample frame.
        public virtual int GetChannelsInBlock();
        public virtual uint GetDacqTriggerSource(uint dacqtrigger_channel, out DigitalSourceEnumNet dacqtrigger_source, out int bitnumber_offset);
        public virtual uint GetDataFormat(uint virtualDevice, DacqGroupChannelEnumNet group, out int numberOfBits);
        //
        // Summary:
        //     Gets the data mode, can be 16, 24 or 32bit, all signed or unsigned on the MEA2100
        //     device.
        //
        // Parameters:
        //   VirtualDevice:
        //     Virtual device to use.
        //
        // Returns:
        //     DataModeEnumNet which enumerates the possible data modes.
        public virtual DataModeEnumNet GetDataMode(uint VirtualDevice);
        public virtual uint GetDigoutSource(uint digout_channel, out DigitalSourceEnumNet digout_source, out int bitnumber_offset);
        public virtual uint GetDigstreamSource(uint digstream_channel, out DigitalSourceEnumNet digstream_source, out int bitnumber_offset);
        public virtual FilterConfiguration GetFilterConfiguration(DacqGroupChannelEnumNet GroupID, uint index);
        public virtual FilterConfiguration[] GetFilterConfigurations(DacqGroupChannelEnumNet GroupID);
        //
        // Summary:
        //     Read data from a FIFO queue in int16_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   group:
        //     Group selector supported by the device.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of int16_t arrays and hardware channel as key.
        public virtual Dictionary<int, short[]> GetGroupChannelDataI16(DacqGroupChannelEnumNet group, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in int32_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   group:
        //     Group selector supported by the device.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of int32_t arrays and hardware channel as key.
        public virtual Dictionary<int, int[]> GetGroupChannelDataI32(DacqGroupChannelEnumNet group, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in uint16_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   group:
        //     Group selector supported by the device.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of uint16_t arrays and hardware channel as key.
        public virtual Dictionary<int, ushort[]> GetGroupChannelDataUI16(DacqGroupChannelEnumNet group, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format, that contains subqueues,
        //     each populates an entry in the dictionary by hardware channel number
        //
        // Parameters:
        //   group:
        //     Group selector supported by the device.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Dictonary of uint32_t arrays and hardware channel as key.
        public virtual Dictionary<int, uint[]> GetGroupChannelDataUI32(DacqGroupChannelEnumNet group, int frames, out int frames_ret);
        public virtual uint GetHardwareMaxRange(uint virtualDevice, DacqGroupChannelEnumNet group, out int r, out int rUnit);
        public virtual uint GetHardwareMinRange(uint virtualDevice, DacqGroupChannelEnumNet group, out int r, out int rUnit);
        //
        // Summary:
        //     Gets the maximal sampling frequency of the device.
        //
        // Returns:
        //     Sampling frequency in Hz.
        public virtual uint GetMaxSamplingFrequency();
        //
        // Summary:
        //     Gets the MEA layout which is connected to the MEA2100 device.
        //
        // Returns:
        //     MeaLayoutEnumNet which enumerates the MEA types.
        public virtual MeaLayoutEnumNet GetMeaLayout();
        //
        // Summary:
        //     Gets the minimal sampling frequency step size increment value of the device.
        //
        // Returns:
        //     Sampling frequency step size in Hz.
        public virtual uint GetMinSamplingFrequencyStepsize();
        //
        // Summary:
        //     Get the real number of data bits.
        //
        // Remarks:
        //     This value may be different from the value returned by GetDataFormat, e.g. in
        //     MC_Card the data are shifted 2 bits so the real number is 14 while the data format
        //     is 16 bits
        public virtual uint GetNumberOfDataBits(uint virtualDevice, DacqGroupChannelEnumNet group, out int numberOfBits);
        public virtual uint GetPoti(uint channel, out uint value);
        public virtual uint GetResolutionPerDigit(uint virtualDevice, DacqGroupChannelEnumNet group, out int res, out int resUnit);
        //
        // Summary:
        //     Gets the sampling frequency of the device.
        //
        // Returns:
        //     Sampling frequency in Hz.
        public virtual int GetSampleRate(int VirtualDevice);
        public virtual uint GetVoltageRangeIndex(uint VirtualDevice);
        //
        // Summary:
        //     Gets the currently selected voltage range on devices which support multiple voltage
        //     ranges.
        //
        // Returns:
        //     The Voltage Range in uV.
        public virtual int GetVoltageRangeInMicroVolt();
        //
        // Summary:
        //     Gets the currently selected voltage range on devices which support multiple voltage
        //     ranges.
        //
        // Returns:
        //     The rounded Voltage Range in mV.
        public virtual int GetVoltageRangeInMilliVolt();
        public CHWInfo HWInfo();
        //
        // Summary:
        //     Start sampling.
        public virtual void SendStart();
        //
        // Summary:
        //     Start sampling.
        //
        // Parameters:
        //   trigger_map:
        //
        //   VirtualDacqMap:
        public virtual void SendStart(int trigger_map, int VirtualDacqMap);
        //
        // Summary:
        //     Stop sampling.
        public virtual void SendStop();
        //
        // Summary:
        //     Stop sampling.
        //
        // Parameters:
        //   trigger_map:
        public virtual void SendStop(int trigger_map);
        //
        // Summary:
        //     Stop sampling.
        //
        // Parameters:
        //   trigger_map:
        //
        //   options:
        public virtual void SendStop(int trigger_map, int options);
        //
        // Summary:
        //     Stop sampling.
        //
        // Parameters:
        //   trigger_map:
        //
        //   options:
        //
        //   VirtualDacqMap:
        public virtual void SendStop(int trigger_map, int options, int VirtualDacqMap);
        public virtual uint SetDacqTriggerSource(uint dacqtrigger_channel, DigitalSourceEnumNet dacqtrigger_source, int bitnumber_offset);
        //
        // Summary:
        //     Sets the data mode, can be 16, 24 or 32bit, all signed or unsigned on the MEA2100
        //     device.
        //
        // Parameters:
        //   dataMode:
        //     DataModeEnumNet enumerates the possible data modes.
        //
        //   virtualDevice:
        //     Virtual device to use.
        public virtual void SetDataMode(DataModeEnumNet dataMode, uint virtualDevice);
        public virtual uint SetDigoutSource(uint digout_channel, DigitalSourceEnumNet digout_source, int bitnumber_offset);
        public virtual uint SetDigstreamSource(uint digstream_channel, DigitalSourceEnumNet digstream_source, int bitnumber_offset);
        public virtual uint SetPoti(uint channel, uint value, bool write_nvram);
        //
        // Summary:
        //     Sets the sampling frequency of the device.
        //
        // Parameters:
        //   rate:
        //     Sampling frequency in Hz.
        public virtual void SetSampleRate(int rate, uint oversample, int VirtualDevice);
        //
        // Summary:
        //     Create a FIFO queue per channel. Each channel will have its own FIFO and Callback
        //     function.
        //
        // Parameters:
        //   nChannels:
        //     Number of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of samples frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        //   ChannelsInBlock:
        //     value obtained from GetChannelsInBlock.
        //
        // Remarks:
        //     When using a 32bit sample size, the number obtained from GetChannelsInBlock must
        //     be devided by 2 to be used here, since GetChannelsInBlock returns the number
        //     of 16 bit datapoints per sample frame, while this functions uses the number of
        //     sample frames in its own data format.
        public virtual void SetSelectedChannels(int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, int ChannelsInBlock);
        //
        // Summary:
        //     Create a FIFO queue per channel. Each channel will have its own FIFO and Callback
        //     function.
        //
        // Parameters:
        //   selectedChannels:
        //     List of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        //   ChannelsInBlock:
        //     value obtained from GetChannelsInBlock.
        //
        // Remarks:
        //     When using a 32bit sample size, the number obtained from GetChannelsInBlock must
        //     be devided by 2 to be used here, since GetChannelsInBlock returns the number
        //     of 16 bit datapoints per sample frame, while this functions uses the number of
        //     sample frames in its own data format.
        public virtual void SetSelectedChannels(bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, int ChannelsInBlock);
        public virtual void SetSelectedChannels(int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize, int ChannelsInBlock);
        public virtual void SetSelectedChannels(bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize, int ChannelsInBlock);
        //
        // Summary:
        //     Create a common FIFO queue for all channels. Data in callback will be a list
        //     per channel. Use ChannelBlock_ReadFramesDict... with handle = 0 to read the data.
        //
        // Parameters:
        //   nChannels:
        //     Number of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        //   ChannelsInBlock:
        //     value obtained from GetChannelsInBlock.
        //
        // Remarks:
        //     When using 32 bit data format, ChannelsInBlock is still the number of 16 bit
        //     channels per frame, as obtained from GetChannelsInBlock, while nChannels is the
        //     number of 32 bit channels to be read from the device. So when all channels from
        //     a device are read in 32 bit data format nChannels = ChannelsInBlock/2
        public virtual void SetSelectedChannelsQueue(int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, int ChannelsInBlock);
        // ττ
        // Summary:
        //     Create a common FIFO queue for all channels. Data in callback will be a list
        //     per channel. Use ChannelBlock_ReadFramesDict... with handle = 0 to read the data.
        //
        // Parameters:
        //   selectedChannels:
        //     List of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        //   ChannelsInBlock:
        //     value obtained from GetChannelsInBlock.
        //
        // Remarks:
        //     When using 32 bit data format, ChannelsInBlock is still the number of 16 bit
        //     channels per frame, as obtained from GetChannelsInBlock, while nChannels is the
        //     number of 32 bit channels to be read from the device. So when all channels from
        //     a device are read in 32 bit data format nChannels = ChannelsInBlock/2
        public virtual void SetSelectedChannelsQueue(bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, int ChannelsInBlock);
        public virtual void SetSelectedChannelsQueue(bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize, int ChannelsInBlock);
        public virtual void SetSelectedChannelsQueue(int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize, int ChannelsInBlock);

        // λ
        // Summary:
        //     Create a common FIFO queue for all channels. Use handle = 0 in the ChannelBlock_ReadFrames...
        //     functions.
        //
        // Parameters:
        //   nChannels:
        //     Number of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        //   ChannelsInBlock:
        //     value obtained from GetChannelsInBlock.
        //
        // Remarks:
        //     When using 32 bit data format, ChannelsInBlock is still the number of 16 bit
        //     channels per frame, as obtained from GetChannelsInBlock, while nChannels is the
        //     number of 32 bit channels to be read from the device. So when all channels from
        //     a device are read in 32 bit data format nChannels = ChannelsInBlock/2
        public virtual void SetSelectedData(int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, int ChannelsInBlock);
        //
        // Summary:
        //     Create a common FIFO queue for all channels. Use handle = 0 in the ChannelBlock_ReadFrames...
        //     functions.
        //
        // Parameters:
        //   selectedChannels:
        //     List of channels to be collected in the FIFO.
        //
        //   queuesize:
        //     Size of sample frames the FIFO can hold.
        //
        //   threshold:
        //     Number of sample frames the FIFO must acquire before the callback function is
        //     called.
        //
        //   samplesize:
        //     size of the datawords, either 16 or 32bit.
        //
        //   ChannelsInBlock:
        //     value obtained from GetChannelsInBlock.
        //
        // Remarks:
        //     When using 32 bit data format, ChannelsInBlock is still the number of 16 bit
        //     channels per frame, as obtained from GetChannelsInBlock, while nChannels is the
        //     number of 32 bit channels to be read from the device. So when all channels from
        //     a device are read in 32 bit data format nChannels = ChannelsInBlock/2
        public virtual void SetSelectedData(bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, int ChannelsInBlock);
        public virtual void SetSelectedData(int nChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize, int ChannelsInBlock);
        public virtual void SetSelectedData(bool[] selectedChannels, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize, int ChannelsInBlock);
        public void SetupGroupDacqQueue(int queuesize, int threshold);
        //
        // Summary:
        //     Sets the voltage range on devices which support multiple voltage ranges.
        //
        // Parameters:
        //   voltageRangeIndex:
        //     Voltage Range to use as index, smaller values are larger voltage ranges.
        public virtual void SetVoltageRangeByIndex(int voltageRangeIndex, uint VirtualDevice);
        //
        // Summary:
        //     Sets the voltage range on devices which support multiple voltage ranges.
        //
        // Parameters:
        //   voltageRange:
        //     Voltage Range to use in µV.
        //
        // Remarks:
        //     This replaces SetVoltageRange, where the value of the range was in mV!
        public virtual void SetVoltageRangeInMicroVolt(int voltageRange, uint VirtualDevice);
        //
        // Summary:
        //     Start the data acquisition thread and sampling.
        public virtual void StartDacq();
        //
        // Summary:
        //     Start the data acquisition thread and sampling.
        //
        // Parameters:
        //   timeout:
        //     Timeout in ms.
        public virtual void StartDacq(int timeout);
        //
        // Summary:
        //     Start the data acquisition thread and sampling.
        //
        // Parameters:
        //   timeout:
        //     Timeout in ms.
        //
        //   numSubmittedUsbBuffers:
        //     Number of USB Buffers that are simultaniously submitted.
        //
        //   numUsbBuffers:
        //     Number of USB Buffers to use.
        //
        //   packetsInUrb:
        //     Packets in each URB.
        public virtual void StartDacq(int timeout, int numSubmittedUsbBuffers, int numUsbBuffers, int packetsInUrb);
        //
        // Summary:
        //     Start the data acquisition thread and sampling.
        //
        // Parameters:
        //   numSubmittedUsbBuffers:
        //     Number of USB Buffers that are simultaniously submitted.
        //
        //   timeout:
        //     Timeout in ms.
        //
        //   numUsbBuffers:
        //     Number of USB Buffers to use.
        //
        //   packetsInUrb:
        //     Packets in each URB.
        //
        //   VirtualDevice:
        //     Virtual Device to start.
        public virtual void StartDacq(int timeout, int numSubmittedUsbBuffers, int numUsbBuffers, int packetsInUrb, uint VirtualDevice);
        //
        // Summary:
        //     Start the data acquisition thread.
        public virtual void StartLoop();
        //
        // Summary:
        //     Start the data acquisition thread.
        //
        // Parameters:
        //   timeout:
        //     Timeout in ms.
        public virtual void StartLoop(int timeout);
        //
        // Summary:
        //     Start the data acquisition thread.
        //
        // Parameters:
        //   timeout:
        //     Timeout in ms.
        //
        //   numSubmittedUsbBuffers:
        //     Number of USB Buffers that are simultaniously submitted.
        //
        //   numUsbBuffers:
        //     Number of USB Buffers to use.
        //
        //   packetsInUrb:
        //     Packets in each URB.
        public virtual void StartLoop(int timeout, int numSubmittedUsbBuffers, int numUsbBuffers, int packetsInUrb);
        //
        // Summary:
        //     Start the data acquisition thread.
        //
        // Parameters:
        //   numSubmittedUsbBuffers:
        //     Number of USB Buffers that are simultaniously submitted.
        //
        //   timeout:
        //     Timeout in ms.
        //
        //   numUsbBuffers:
        //     Number of USB Buffers to use.
        //
        //   packetsInUrb:
        //     Packets in each URB.
        //
        //   VirtualDevice:
        //     Virtual Device to start.
        public virtual void StartLoop(int timeout, int numSubmittedUsbBuffers, int numUsbBuffers, int packetsInUrb, uint VirtualDevice);
        //
        // Summary:
        //     Stop the data acquisition thread and sampling.
        public virtual void StopDacq();
        //
        // Summary:
        //     Stop the data acquisition thread and sampling.
        //
        // Parameters:
        //   VirtualDevice:
        //     Virtual Device to start.
        public virtual void StopDacq(uint VirtualDevice);
        public virtual void StopLoop();
        protected virtual uint ChannelBlock_AddChannel(int handle, int offset, int channels);
        protected virtual uint ChannelBlock_AddChannel(int handle, int queue, int offset, int channels);
        protected virtual uint ChannelBlock_AddQueue(out int handle, int queuesize, int threshold, SampleSizeNet samplesize);
        protected virtual uint ChannelBlock_AddQueue(out int handle, int number_of_queues, int queuesize, int threshold, SampleSizeNet samplesize);
        protected virtual uint ChannelBlock_AddQueue(out int handle, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize);
        protected virtual uint ChannelBlock_AddQueue(out int handle, int number_of_queues, int queuesize, int threshold, SampleSizeNet samplesize, SampleDstSizeNet sampleDstSize);
        protected virtual uint ChannelBlock_GetChannel(int handle, int queue, int channelentry, out int totalchannels, out int offset, out int channels);
        protected virtual int ChannelBlock_GetNumberOfQueues(int handle);
        protected virtual uint ChannelBlock_Init(int words_per_sample);
        //
        // Summary:
        //     Read data from a FIFO queue in int16_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   queue:
        //     Handle of the queue.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        protected virtual short[] ChannelBlock_ReadFramesI16(int handle, int queue, int frames, out int frames_ret);


        // µ
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        protected virtual int[] ChannelBlock_ReadFramesI32(int handle, int queue, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in uint16_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        //
        // Returns:
        //     Array of data from the device.
        protected virtual ushort[] ChannelBlock_ReadFramesUI16(int handle, int queue, int frames, out int frames_ret);
        //
        // Summary:
        //     Read data from a FIFO queue in uint32_t data format
        //
        // Parameters:
        //   handle:
        //     Handle of the FIFO queue. Either zero when the SetSelectedData call was used
        //     or the channel number.
        //
        //   frames:
        //     Number of sample frames to read.
        //
        //   frames_ret:
        //     Number of sample frames which were read, might be smaller than frames.
        protected virtual uint[] ChannelBlock_ReadFramesUI32(int handle, int queue, int frames, out int frames_ret);
        protected virtual uint ChannelBlock_SetQueueSize(int handle, int queuesize, int callbackThreshold);
        protected virtual uint ChannelBlock_SetQueueSizeAll(int queuesize, int callbackThreshold);
        protected virtual uint ChannelBlock_UpdateQueueSize(int handle);
        [HandleProcessCorruptedStateExceptions]
        protected override void Dispose(bool A_0);
        protected void InitCont();
        protected void raise_ChannelDataEvent(CMcsUsbDacqNet value0, int value1, int value2);
        protected void raise_ErrorEvent(string value0, int value1);

        //
        // Summary:
        //     Class to provide hardware information about the device.
        public class CHWInfo
        {
            public CHWInfo(CMcsUsbDacqNet device);

            public virtual uint GetAvailableSampleRates(out List<int> sampleRates);
            public virtual uint GetAvailableVoltageRangesInMicroVolt(out List<int> voltageRanges);
            public virtual uint GetAvailableVoltageRangesInMicroVoltAndStringsInMilliVolt(out List<CVoltageRangeInfoNet> voltageRanges);
            //
            // Summary:
            //     Get the number of analog channels the device supports.
            //
            // Parameters:
            //   numberOfChannels:
            //     Number of analog channels the device supports.
            //
            // Returns:
            //     Error Status. 0 on success.
            public virtual uint GetNumberOfHWADCChannels(out int numberOfChannels);
            //
            // Summary:
            //     Get the number of digital channels the device supports.
            //
            // Parameters:
            //   numberOfChannels:
            //     Number of digital channels the device supports.
            //
            // Returns:
            //     Error Status. 0 on success.
            public virtual uint GetNumberOfHWDigitalChannels(out int numberOfChannels);
            //
            // Summary:
            //     Query if the digital channel replaces an analog channel when enabled (e.g. on
            //     MC_Card) or adds a channel link on USB devices.
            //
            // Returns:
            //     false when the digital channel replaces an analog channel when enabled, true
            //     when the digital channels is appended to the analog channels when enabled.
            public virtual bool IsDigitalChannelDedicated();

            public class CVoltageRangeInfoNet
            {
                public string VoltageRangeDisplayStringMilliVolt;
                public int VoltageRangeInMicroVolt;

                public CVoltageRangeInfoNet(int vr, string vrString);
            }
        }
    }
}
