using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace ReliableUdp.Example
{
    class Example1
    {
        public static void Start()
        {
            SimpleServer server = new SimpleServer(555);
            server.Start();
            
            SimpleClient client1 = new SimpleClient();
            client1.Start(new IPEndPoint(IPAddress.Loopback, 555), MethodType.Typical);
            SimpleClient client2 = new SimpleClient();
            client2.Start(new IPEndPoint(IPAddress.Loopback, 555), MethodType.Typical);
            SimpleClient client3 = new SimpleClient();
            client3.Start(new IPEndPoint(IPAddress.Loopback, 555), MethodType.Typical);

            Console.WriteLine("++++++++++++ Example 1 ++++++++++++");
            Console.WriteLine("============Client Send============");
            client1.Send("Hello, I'm 1st", true);
            Thread.Sleep(100);
            client2.Send("Hello, I'm 2nd", true);
            Thread.Sleep(100);
            client3.Send("Hello, I'm 3rd", true);
            Thread.Sleep(100);
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
