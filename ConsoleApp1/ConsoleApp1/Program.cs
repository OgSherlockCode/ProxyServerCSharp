using EasyNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer
{
    class Program
    {
        static NetworkComponent server = new NetworkComponent();
        static void Main(string[] args)
        {
            server.Connected += new EventHandler<Socket>(Connect);
            server.StartServer("127.0.0.1", 1080, ProtocolType.Tcp);
        }

        private static void Connect(object sender, Socket e)
        {
            try
            {
                byte[] buf = server.Recive(e);
                if (buf[0] == 0x04)
                    Socks4(buf, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }


        private static void Socks4(byte[] buf, Socket e)
        {
            int port = BitConverter.ToInt16(new byte[] { buf[3], buf[2] }, 0);
            IPAddress ip = IPAddress.Parse(String.Format("{0}.{1}.{2}.{3}", buf[4], buf[5], buf[6], buf[7]));
            Socket webServer = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            switch (buf[1])
            {
                case 0x01:
                    //Connect to ip
                    webServer.Connect(ip, port);
                    e.Send(new byte[] { 0x00, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    new Thread(() => { ExchangeData(webServer, e); }).Start();
                    ExchangeData(e, webServer);
                    break;

                case 0x02:
                    //Bind ip port
                    webServer.Bind(new IPEndPoint(ip, port));
                    e.Send(new byte[] { 0x00, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    new Thread(() => { ExchangeData(webServer, e); }).Start();
                    ExchangeData(e, webServer);
                    break;

                default:
                    return;
            }
        }


        static void ExchangeData(Socket from,Socket to)
        {
            try
            {
                while (true)
                {
                    byte[] buf = server.Recive(from);
                    if (buf.Length == 0) break;
                    to.Send(buf);
                }
            }
            catch
            {
                from.Disconnect(false);
                to.Disconnect(false);
            }
            
        }
    }
}
