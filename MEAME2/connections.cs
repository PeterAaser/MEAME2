using System;
using System.IO;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace MEAME2
{
  public class ConnectionManager
  {
    public Action newConnectionAction;

    public List<Socket> ChannelListeners = new List<Socket>();
    public List<Socket> SawToothListeners = new List<Socket>();
    public DAQ daq;
    public int[] channels;
    private int throttle = 498;
    private int testInt = -4000;
    private int nChannels = 60;

    private long periodCounter = 0;

		private bool hasLogged = false;

    // BlockCopy(Array SOURCE,
    //           int SOURCE OFFSET IN BYTES,
    //           Array DESTINATION,
    //           int DESTINATION OFFSET IN BYTES,
    //           int BYTES TO COPY
    // public void OnChannelData(Dictionary<int, int[]> data, int returnedFrames){
    public void OnChannelData(int[] data, int returnedFrames){

      int[] reordered = new int[returnedFrames*60];

      if(ChannelListeners.Any()){
        byte[] sendBuffer = new byte[returnedFrames * 60 * 4];

        for(int channelNo = 0; channelNo < 60; channelNo++){

          int reorderedOffset = channelNo*returnedFrames;

      	  // What does it mean that this is 64??
	        // Does the data arrive with segment length 1? What are the 4 bonus channels?
          int rawStride = 64;
          int rawOffset = channelNo;

          for(int dataPointNo = 0; dataPointNo < returnedFrames; dataPointNo++){
            reordered[reorderedOffset + dataPointNo] = data[rawOffset + rawStride*dataPointNo];
          }
        }

				if(!hasLogged){
						log.info($"{data[64*0]}");
						log.info($"{data[64*1]}");
						log.info($"{data[64*2]}");
						log.info($"{data[64*3]}");
						log.info($"{data[64*4]}");
						log.info($"{data[64*5]}");
						log.info($"{data[64*6]}");
						log.info($"{data[64*7]}");
						log.info($"{data[64*8]}");
						log.info($"{data[64*9]}");

						
						log.info($"{reordered[0]}");
						log.info($"{reordered[1]}");
						log.info($"{reordered[2]}");
						log.info($"{reordered[3]}");
						log.info($"{reordered[4]}");
						log.info($"{reordered[5]}");
						log.info($"{reordered[6]}");
						log.info($"{reordered[7]}");
						log.info($"{reordered[8]}");
						log.info($"{reordered[9]}");
					hasLogged = true;
				}


        Buffer.BlockCopy(reordered, 0, sendBuffer, 0, returnedFrames*4*60);

        /**
           Send data to listeners of all channels
           Removes disconnected listeners
        */
        for(int ii = ChannelListeners.Count - 1; ii >= 0; ii--){
          try {
            ChannelListeners[ii].Send(sendBuffer);
          }
          catch (SocketException e) {
            ChannelListeners.RemoveAt(ii);
            log.info($"removed channel listener {ii}");
            log.info($"now {ChannelListeners.Count} listeners");
          }
        }
      }
      if(SawToothListeners.Any()){
        broadCastSawTooth(returnedFrames);
      }
    }


    // BlockCopy(Array SOURCE,
    //           int SOURCE OFFSET IN BYTES,
    //           Array DESTINATION,
    //           int DESTINATION OFFSET IN BYTES,
    //           int BYTES TO COPY
    private void broadCastSawTooth(int returnedFrames){

      byte[] sendBuffer = new byte[returnedFrames * 60 * 4];
      int[] waveform = new int[returnedFrames * 60];
      long absPoint = returnedFrames*(periodCounter);
      periodCounter += 1;

      for (int ii = 0; ii < 60; ii++){

        int waveformIdxOffset = ii*returnedFrames;
        // channels 0 - 19 sawtooth from -400 to 400, channels 20 - 39 from -800 to 800 etc
        int channelMax = ((ii/20) + 1)*400;
        int slope = (ii%20) + 1;
        int byteOffset = ii*returnedFrames*4;

        if(ii == 14){
          for(int jj = 0; jj < returnedFrames; jj++){
            waveform[jj + waveformIdxOffset] = 0;
          }
        }
        else{
          // adds sawtooth waves (obv)
          int startPoint = (int)((absPoint*slope) % (2*channelMax));
          for(int jj = 0; jj < returnedFrames; jj++){

            int point = (startPoint + (jj*slope)) % (2*channelMax);

            waveform[jj + waveformIdxOffset] = point - channelMax;
          }
        }
      }
      Buffer.BlockCopy(waveform, 0, sendBuffer, 0, returnedFrames*4*60);


      /**
         Send data to listeners of all channels
         Removes disconnected listeners
      */
      for(int ii = SawToothListeners.Count - 1; ii >= 0; ii--){
        try {
          SawToothListeners[ii].Send(sendBuffer);
        }
        catch (SocketException e) {
          SawToothListeners.RemoveAt(ii);
          log.info($"removed sawtooth listener {ii}");
          log.info($"now {SawToothListeners.Count} listeners");
        }
      }
    }
  }
}
