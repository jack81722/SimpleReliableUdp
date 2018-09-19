# SimpleReliableUdp
An app-to-app reliable udp.

Features:
1. Light and easy to use becuase it inherited from UdpClient.
2. Use Seq/Ack mechanism to resend packet.

How to Use:
1. New a ReliableUdpClient between two peers
2. Use Send/SendAsync and Receive/ReceiveAsync to deliver packets

Attention:
1. Don't use BeginSend/BeginReceive because not fixed.
2. The Seq/Ack number only support 0~255, it will throw exception if number of resend event is over 255
3. It will have problem if you use normal udp socket
