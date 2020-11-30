using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EasyNetworking
{
    public class NetworkComponent
    {
        public event EventHandler<Socket> Connected;
        public void StartServer(string ip = "127.0.0.1", int port = 5000, ProtocolType protocolType = ProtocolType.Tcp)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, protocolType);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                Console.WriteLine("Waiting for a connection...");
                while (true)
                {
                    Socket handler = listener.Accept();
                    new Thread(() => { Connected?.Invoke(this, handler); }).Start();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }


        public int chunkSize = 1024;
        public byte[] Recive(Socket socket)
        {
            byte[] buffer = new byte[chunkSize];
            int size = chunkSize;
            List<byte> endBuffer = new List<byte>();
            while (size == chunkSize)
            {
                size = socket.Receive(buffer, 0, chunkSize, SocketFlags.None);
                if (size != chunkSize)
                    Array.Resize(ref buffer, size);
                endBuffer.AddRange(buffer);
            }
            return endBuffer.ToArray();
        }
    }
}
