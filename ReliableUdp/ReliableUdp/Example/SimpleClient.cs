using Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Example
{
    public class SimpleClient
    {
        private static int port = 556;
        private ReliableUdpClient client;
        private IPEndPoint destPoint;
        private bool connected;

        private Task recvTask;
        
        public SimpleClient()
        {
            client = new ReliableUdpClient(port++);
            connected = false;
        }

        public void Start(IPEndPoint endPoint, MethodType type)
        {
            destPoint = endPoint;
            switch (type)
            {
                case MethodType.Typical:
                    recvTask = Task.Factory.StartNew(Recv);
                    break;
                case MethodType.Async:
                    client.Connect(destPoint);
                    connected = true;
                    recvTask = Task.Factory.StartNew(RecvAsync);
                    break;
                //case MethodType.Begin:
                //    recvTask = Task.Factory.StartNew(BeginRecv);
                //    break;
                default:
                    recvTask = Task.Factory.StartNew(Recv);
                    break;
            }
        }

        public void Send(string message, bool reliable)
        {
            byte[] data = Serialization.ToByteArray(message);
            if (connected)
                client.Send(data, data.Length, reliable);
            else
                client.Send(data, data.Length, destPoint, reliable);
        }

        public void SendAsync(string message, bool reliable)
        {
            byte[] data = Serialization.ToByteArray(message);
            if (connected)
                client.SendAsync(data, data.Length, reliable);
            else
                client.SendAsync(data, data.Length, destPoint, reliable);
        }

        public void BeginSend(string message, bool reliable)
        {
            byte[] data = Serialization.ToByteArray(message);
            if (connected)
                client.BeginSend(data, data.Length, SendCallback, client, reliable);
            else
                client.BeginSend(data, data.Length, destPoint, SendCallback, client, reliable);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            ReliableUdpClient u = (ReliableUdpClient)ar.AsyncState;

            Console.WriteLine("number of bytes sent: {0}", u.EndSend(ar));
        }

        public void Recv()
        {
            while (true)
            {
                try
                {
                    byte[] data = client.Receive(ref destPoint);
                    Console.WriteLine("Client receive \"" + Serialization.ToObject<string>(data) + "\" from " + destPoint.ToString());
                }
                catch
                {
                    Console.WriteLine("Client end recv");
                    break;
                }
            }
        }
        
        public async void RecvAsync()
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult data = await client.ReceiveAsync();
                    Console.WriteLine("Client receive \"" + Serialization.ToObject<string>(data.Buffer) + "\" from " + data.RemoteEndPoint.ToString());
                }
                catch
                {
                    Console.WriteLine("Client end recv");
                    break;
                }
            }
        }

        public void BeginRecv()
        {
            while (true)
            {
                try
                {
                    client.BeginReceive(ReceiveCallback, new UdpState() { u = client, e = destPoint });
                }
                catch
                {
                    Console.WriteLine("Client end recv");
                    break;
                }
            }
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            ReliableUdpClient u = (ReliableUdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Serialization.ToObject<string>(receiveBytes);

            Console.WriteLine("Received: {0}", receiveString);
        }

        public struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }

        public void Close()
        {
            client.Close();
            recvTask.Wait();
        }
    }
}
