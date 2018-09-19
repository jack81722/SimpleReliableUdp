using Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Example
{
    class SimpleServer
    {
        private ReliableUdpClient server;
        private IPEndPoint endPoint;
        private List<IPEndPoint> endPointList;

        private Task recvTask;

        public SimpleServer(int port)
        {
            server = new ReliableUdpClient(new IPEndPoint(IPAddress.Any, port));
            endPointList = new List<IPEndPoint>();
        }

        public void Start()
        {
            recvTask = Task.Factory.StartNew(Recv);
        }

        public void Recv()
        {
            while (true)
            {
                try
                {
                    byte[] result = server.Receive(ref endPoint);
                    if (!endPointList.Contains(endPoint))
                    {
                        Console.WriteLine("Got a client : " + endPoint);
                        endPointList.Add(endPoint);
                    }
                    Console.WriteLine("Server receive \"" + Serialization.ToObject<string>(result) + "\" from " + endPoint.ToString());
                }
                catch
                {
                    Console.WriteLine("Server end recv");
                    break;
                }
            }
        }
        
        public void Send(string message)
        {
            byte[] data = Serialization.ToByteArray(message);
            for(int i = 0; i < endPointList.Count; i++)
                server.Send(data, data.Length, endPointList[i]);
        }

        public void Close()
        {
            server.Close();
            recvTask.Wait();
        }
    }
}
