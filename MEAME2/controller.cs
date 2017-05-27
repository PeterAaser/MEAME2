using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MEAME2
{
  public class MEAMEcontrol
  {
    private ConnectionManager cm;
    private ChannelServer     channelServer;
    private DAQ               daq;
    // private STG

    // hardcoded
    int samplerate = 40000;
    int channelBlockSize = 1024;

    public MEAMEcontrol(){
      this.cm = new ConnectionManager();
      this.channelServer = new ChannelServer(cm);

      // won't compile on GNU/loonix
      // this.daq = new DAQ{
      //   samplerate = 40000,
      //   channelBlockSize = 128,
      //   onChannelData = cm.OnChannelData
      // };
    }

    public bool startServer(){
      return true;
    }

    public bool connectDAQ(DAQconfig d){
      // ayy
      return true;
    }
  }
}
