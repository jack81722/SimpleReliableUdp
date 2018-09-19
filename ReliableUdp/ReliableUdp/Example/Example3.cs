using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace ReliableUdp.Example
{
    class Example3
    {
        public static void Start()
        {
            SimpleServer server = new SimpleServer(777);
            server.Start();

            SimpleClient client1 = new SimpleClient();
            client1.Start(new IPEndPoint(IPAddress.Loopback, 777), MethodType.Async);
            SimpleClient client2 = new SimpleClient();
            client2.Start(new IPEndPoint(IPAddress.Loopback, 777), MethodType.Async);
            SimpleClient client3 = new SimpleClient();
            client3.Start(new IPEndPoint(IPAddress.Loopback, 777), MethodType.Typical);

            Console.WriteLine();
            Console.WriteLine("++++++++++++ Example 3 ++++++++++++");
            Console.WriteLine("============Client Send============");
            for (int i = 0; i < 100; i++)
            {
                client1.Send("Hello, I'm 1st", true);
                Thread.Sleep(10);
                client2.Send("Hello, I'm 2nd", true);
                Thread.Sleep(10);
                client3.Send("Hello, I'm 3rd", true);
                Thread.Sleep(10);
            }
            Console.WriteLine("============Server Send============");
            server.Send("Bye");

            Thread.Sleep(1000);
            client1.Close();
            client2.Close();
            client3.Close();
            server.Close();
        }
    }
}
