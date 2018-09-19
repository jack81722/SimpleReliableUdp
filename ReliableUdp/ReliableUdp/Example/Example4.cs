using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ReliableUdp.Example
{
    class Example4
    {
        public static void Start()
        {
            SimpleServer server = new SimpleServer(888);
            server.Start();

            SimpleClient client1 = new SimpleClient();
            client1.Start(new IPEndPoint(IPAddress.Loopback, 888), MethodType.Begin);

            Console.WriteLine();
            Console.WriteLine("++++++++++++ Example 4 ++++++++++++");
            Console.WriteLine("============Client Send============");
            for (int i = 0; i < 100; i++)
            {
                client1.BeginSend("Hello", true);
                Thread.Sleep(10);
               
            }
            Console.WriteLine("============Server Send============");
            server.Send("Bye");

            Thread.Sleep(1000);
            client1.Close();
            server.Close();
        }

        
    }
}
