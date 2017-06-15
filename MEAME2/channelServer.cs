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
    private static int selectiveChannelsPort = 12341;

    public ChannelServer(ConnectionManager cm) {
      this.cm = cm;
    }


    public void startListener()
    {
      Thread listener = new Thread( () => allChannelListener(cm) );
      Thread listener2 = new Thread( () => selectiveChannelListener(cm) );

      listener.Start();
      listener2.Start();
    }


    private static void allChannelListener(ConnectionManager cm) {
      Console.WriteLine("starting all channel listener");
      IPAddress myip;
      IPAddress.TryParse(ipstring, out myip);
      Socket listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

      listener.Bind(new IPEndPoint(myip, allChannelsPort));
      listener.Listen(10);

      while (true){
        Console.WriteLine($"Server is listening on port {allChannelsPort}");
        Socket connection = listener.Accept();
        Console.WriteLine($"Connection to port {allChannelsPort} accepted. Full data");

        cm.allChannelListeners.Add(connection);
      }

      listener.Close();
      return;
    }


    private static void selectiveChannelListener(ConnectionManager cm) {
      Console.WriteLine("starting selective channel listener");
      IPAddress myip;
      IPAddress.TryParse(ipstring, out myip);
      Socket listener = new Socket(AddressFamily.InterNetwork,
                                   SocketType.Stream,
                                   ProtocolType.Tcp);

      listener.Bind(new IPEndPoint(myip, selectiveChannelsPort));
      listener.Listen(10);

      while (true){
        Console.WriteLine($"Server is listening on port {selectiveChannelsPort}");
        Socket connection = listener.Accept();

        Thread stimThread = new Thread( () => attachReader(connection) );
        stimThread.Start();

        Console.WriteLine($"Connection to port {selectiveChannelsPort} accepted. Selective data");

        cm.selectiveListeners.Add(connection);
      }

      listener.Close();
      return;
    }


    public static void attachReader(Socket socket)
    {
      Console.WriteLine("STIM READER STARTED");
      NetworkStream ns = new NetworkStream(socket);
      StreamReader streamreader = new StreamReader(ns);
      int throttle = 0;

      while (true)
        {
          // string memeString = streamreader.ReadLine();
          // StringReader memeReader = new StringReader(memeString);
          // JsonTextReader memer = new JsonTextReader(memeReader);
          // JsonSerializer serializer = new JsonSerializer();
          // StimReq s = serializer.Deserialize<StimReq>(memer);
          // if(throttle == 40)
          //   {
          //     Console.WriteLine(s);
          //     throttle = 0;
          //   }
          // else
          //   {
          //     throttle++;
          //   }
        }
    }
  }
}
