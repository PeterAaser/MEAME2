using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Mcs.Usb;

namespace MEAME2
{
  public class ChannelServer {

    private static string ipstring = "0.0.0.0";
    private ConnectionManager cm;
    private static int ChannelsPort = 12340;
    private static int SawToothPort = 12341;

    public ChannelServer(ConnectionManager cm) {
      this.cm = cm;
    }


    public void startListener()
    {
      Thread listener = new Thread( () => ChannelListener(cm) );
      Thread listener2 = new Thread( () => SawToothListener(cm) );
      listener.Start();
      listener2.Start();

    }

    private static void ChannelListener(ConnectionManager cm) {

      IPAddress myip;
      IPAddress.TryParse(ipstring, out myip);
      Socket listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

      listener.Bind(new IPEndPoint(myip, ChannelsPort));
      listener.Listen(10);

      while (true){
        Socket connection = listener.Accept();
        log.ok($"Connection to port {ChannelsPort} accepted", "TCP ");

        cm.ChannelListeners.Add(connection);
      }

      listener.Close();
      return;
    }


    private static void SawToothListener(ConnectionManager cm) {

      IPAddress myip;
      IPAddress.TryParse(ipstring, out myip);
      Socket listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

      listener.Bind(new IPEndPoint(myip, SawToothPort));
      listener.Listen(10);

      while (true){
        Socket connection = listener.Accept();
        log.ok($"Connection to port {SawToothPort} accepted", "TCP ");

        cm.SawToothListeners.Add(connection);
      }

      listener.Close();
      return;
    }
  }
}
