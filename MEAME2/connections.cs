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
    public List<Socket> selectiveListeners = new List<Socket>();
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


      if(throttle == 0){
        // Console.WriteLine(totalChannels);
      }
      throttle = (throttle + 1) % 100;



      byte[] sendBuffer = new byte[returnedFrames * 60 * 4];
      for (int ii = 0; ii < 60; ii++){

        int byteOffset = ii*returnedFrames*4;

        Buffer.BlockCopy(data[ii], 0, sendBuffer, byteOffset, returnedFrames*4);
        // Buffer.BlockCopy(channelData, 0, sendBuffer, byteOffset, returnedFrames*4);
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


      /**
         Collect data for those listening only to select channels
      */
      // Console.WriteLine("Rearranging data to be sent to [SELECTIVE CHANNEL] listeners");
      byte[] sendBuffer2 = new byte[channels.Length * returnedFrames * 4];
      for (int ii = 0; ii < channels.Length; ii++){



        int[] source = data[channels[ii]];
        int destByteOffset = ii * returnedFrames * 4;

        Buffer.BlockCopy(source, 0, sendBuffer2, destByteOffset, returnedFrames*4);
      }


      /**
         Send data to listeners on select channels
      */
      // Console.WriteLine("Sending data to [SELECTIVE LISTENERS]");
      for(int ii = selectiveListeners.Count - 1; ii >= 0; ii--){
        try {
          // Console.WriteLine($"{ii}");
          selectiveListeners[ii].Send(sendBuffer2);
        }
        catch (SocketException e) {
          Console.WriteLine("------------------------------------------------------");
          Console.WriteLine("-------------------- SOCKET EXCEPTION ----------------");
          Console.WriteLine("------------------------------------------------------");
          selectiveListeners.RemoveAt(ii);
          this.daq.stopDevice();
        }
      }
    }


    // public void OnChannelDataOld(int mChannelHandles, int[] data, int totalChannels, int returnedFrames)
    // {

    //   /**
    //      Gather data for all channels
    //   */
    //   byte[] sendBuffer = new byte[returnedFrames * totalChannels * 4];
    //   for (int ii = 0; ii < totalChannels; ii++){

    //     int byteOffset = ii*returnedFrames*4;
    //     int[] channelData = new int[returnedFrames];

    //     for (int jj = 0; jj < returnedFrames; jj++){
    //       channelData[jj] = data[jj * mChannelHandles + ii];
    //       // channelData[jj] = 0;
    //     }

    //     Buffer.BlockCopy(dummyData, 0, sendBuffer, byteOffset, returnedFrames*4);
    //     // Buffer.BlockCopy(channelData, 0, sendBuffer, byteOffset, returnedFrames*4);
    //   }

    //   testInt = testInt + 20;
    //   if(testInt % 20 == 0){
    //     Console.WriteLine(testInt);
    //   }


    //   /**
    //      Send data to listeners of all channels
    //   */
    //   // Console.WriteLine("Sending data to [ALL CHANNEL] listeners");
    //   for(int ii = allChannelListeners.Count - 1; ii >= 0; ii--){
    //     try {
    //       allChannelListeners[ii].Send(sendBuffer);
    //     }
    //     catch (SocketException e) {
    //       allChannelListeners.RemoveAt(ii);
    //     }
    //   }


    //   /**
    //      Collect data for those listening only to select channels
    //   */
    //   // Console.WriteLine("Rearranging data to be sent to [SELECTIVE CHANNEL] listeners");
    //   byte[] sendBuffer2 = new byte[channels.Length * returnedFrames * 4];
    //   for (int ii = 0; ii < channels.Length; ii++){

    //     if(ii == 0){
    //       // for(int jj = 0; jj < 10; jj++){
    //       //   Console.Write($"[{channelData[jj]}]");
    //       // }
    //       // Console.WriteLine();
    //     }

    //     int sourceByteOffset = channels[ii] * returnedFrames * 4;
    //     int destByteOffset = ii * returnedFrames * 4;

    //     Buffer.BlockCopy(data, sourceByteOffset, sendBuffer2, destByteOffset, returnedFrames*4);
    //   }


    //   /**
    //      Send data to listeners on select channels
    //   */
    //   // Console.WriteLine("Sending data to [SELECTIVE LISTENERS]");
    //   for(int ii = selectiveListeners.Count - 1; ii >= 0; ii--){
    //     try {
    //       // Console.WriteLine($"{ii}");
    //       selectiveListeners[ii].Send(sendBuffer2);
    //     }
    //     catch (SocketException e) {
    //       Console.WriteLine("------------------------------------------------------");
    //       Console.WriteLine("-------------------- SOCKET EXCEPTION ----------------");
    //       Console.WriteLine("------------------------------------------------------");
    //       selectiveListeners.RemoveAt(ii);
    //       this.daq.stopDevice();
    //     }
    //   }
    // }
  }
}
