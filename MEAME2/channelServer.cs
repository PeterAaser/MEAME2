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

    private static string ipstring = "129.241.201.110";
    private ConnectionManager cm;
    private static int allChannelsPort = 12340;

    public ChannelServer(ConnectionManager cm) {
      this.cm = cm;
    }


    public void startListener()
    {
      Thread listener = new Thread( () => allChannelListener(cm) );

      listener.Start();
    }


    private static void allChannelListener(ConnectionManager cm) {

      IPAddress myip;
      IPAddress.TryParse(ipstring, out myip);
      Socket listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

      listener.Bind(new IPEndPoint(myip, allChannelsPort));
      listener.Listen(10);

      while (true){
        Socket connection = listener.Accept();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[TCP Info]: Connection to port {allChannelsPort} accepted");
        Console.ResetColor();

        cm.allChannelListeners.Add(connection);
      }

      listener.Close();
      return;
    }
  }
}
