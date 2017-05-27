using System;
using System.IO;
using System.Net.Sockets;
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
    public int[] channels;


    // BlockCopy(Array SOURCE,
    //           int SOURCE OFFSET IN BYTES,
    //           Array DESTINATION,
    //           int DESTINATION OFFSET IN BYTES,
    //           int BYTES TO COPY
    public void OnChannelData(int mChannelHandles, int[] data, int totalChannels, int returnedFrames)
    {
      /**
         Gather data for all channels
      */
      byte[] sendBuffer = new byte[returnedFrames * totalChannels * 4];
      for (int ii = 0; ii < totalChannels; ii++){

        int byteOffset = ii*returnedFrames*4;
        int[] channelData = new int[returnedFrames];

        for (int jj = 0; jj < returnedFrames; jj++){
          channelData[jj] = data[jj * mChannelHandles + ii];
        }

        Buffer.BlockCopy(channelData, 0, sendBuffer, byteOffset, returnedFrames*4);
      }


      /**
         Send data to listeners of all channels
      */
      for(int ii = allChannelListeners.Count; ii > 0; --ii){
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
      byte[] sendBuffer2 = new byte[channels.Length * returnedFrames * 4];
      for (int ii = 0; ii < channels.Length; ii++){

        int sourceByteOffset = channels[ii] * returnedFrames * 4;
        int destByteOffset = ii * returnedFrames * 4;

        Buffer.BlockCopy(data, sourceByteOffset, sendBuffer2, destByteOffset, returnedFrames*4);
        // do thing
        return;
      }


      /**
         Send data to listeners on select channels
      */
      for(int ii = selectiveListeners.Count; ii > 0; --ii){
        try {
          selectiveListeners[ii].Send(sendBuffer2);
        }
        catch (SocketException e) {
          selectiveListeners.RemoveAt(ii);
        }
      }
    }
  }
}
