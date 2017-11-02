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


    // BlockCopy(Array SOURCE,
    //           int SOURCE OFFSET IN BYTES,
    //           Array DESTINATION,
    //           int DESTINATION OFFSET IN BYTES,
    //           int BYTES TO COPY
    public void OnChannelData(Dictionary<int, int[]> data, int returnedFrames){

      if(ChannelListeners.Any()){
        byte[] sendBuffer = new byte[returnedFrames * 60 * 4];
        for (int ii = 0; ii < 60; ii++){

          int byteOffset = ii*returnedFrames*4;
          Buffer.BlockCopy(data[ii], 0, sendBuffer, byteOffset, returnedFrames*4);
        }

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

        // adds sawtooth waves (obv)
        int startPoint = (int)((absPoint*slope) % (2*channelMax));
        for(int jj = 0; jj < returnedFrames; jj++){

          int point = (startPoint + (jj*slope)) % (2*channelMax);

          waveform[jj + waveformIdxOffset] = point - channelMax;
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
        }
      }
    }

    // void printStuff( Dictionary<int, int[]> data, int returnedFrames ){
    //   var allKeys = data.Keys.ToArray();
    //   for (int key = 0; key < 64; key++){
    //     Console.WriteLine($"---[{key}]---");
    //     int count = 0;
    //     for (int jj = 0; jj < 90; jj++){
    //       // Console.Write($"[{data[key][jj]}]");
    //       int ghettoAbs = 0;
    //       if(data[key][jj] > 0){
    //         ghettoAbs = data[key][jj];
    //       }else{
    //         ghettoAbs = -data[key][jj];
    //       }
    //       count += ghettoAbs;
    //     }
    //     Console.WriteLine();
    //     Console.WriteLine(count);
    //     Console.WriteLine();
    //   }
    // }


    // private void throttledPrint(String s){
    //   if(throttle == 1000){
    //     Console.ForegroundColor = ConsoleColor.Yellow;
    //     Console.WriteLine($"[CM Info]: {s}");
    //     Console.ResetColor();
    //     throttle = 0;
    //   }
    //   throttle++;
    // }

  }
}
