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

    public List<Socket> allChannelListeners = new List<Socket>();
    public DAQ daq;
    public int[] channels;
    private int throttle = 498;
    private int testInt = -4000;
    private int nChannels = 60;


    void printStuff( Dictionary<int, int[]> data, int returnedFrames ){
      var allKeys = data.Keys.ToArray();
      for (int key = 0; key < 64; key++){
        Console.WriteLine($"---[{key}]---");
        int count = 0;
        for (int jj = 0; jj < 90; jj++){
          // Console.Write($"[{data[key][jj]}]");
          int ghettoAbs = 0;
          if(data[key][jj] > 0){
            ghettoAbs = data[key][jj];
          }else{
            ghettoAbs = -data[key][jj];
          }
          count += ghettoAbs;
        }
        Console.WriteLine();
        Console.WriteLine(count);
        Console.WriteLine();
      }
    }


    // BlockCopy(Array SOURCE,
    //           int SOURCE OFFSET IN BYTES,
    //           Array DESTINATION,
    //           int DESTINATION OFFSET IN BYTES,
    //           int BYTES TO COPY
    public void OnChannelData(Dictionary<int, int[]> data, int returnedFrames){

      throttle = (throttle + 1) % 100;
      byte[] sendBuffer = new byte[returnedFrames * 60 * 4];
      for (int ii = 0; ii < 60; ii++){

        int byteOffset = ii*returnedFrames*4;

        Buffer.BlockCopy(data[ii], 0, sendBuffer, byteOffset, returnedFrames*4);
      }



      /**
         Send data to listeners of all channels
      */
      // Console.WriteLine("Sending data to [ALL CHANNEL] listeners");
      for(int ii = allChannelListeners.Count - 1; ii >= 0; ii--){
        try {
          allChannelListeners[ii].Send(sendBuffer);
        }
        catch (SocketException e) {
          allChannelListeners.RemoveAt(ii);
        }
      }
    }

    private void throttledPrint(String s){
      if(throttle == 1000){
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[CM Info]: {s}");
        Console.ResetColor();
        throttle = 0;
      }
      throttle++;
    }
  }
}
