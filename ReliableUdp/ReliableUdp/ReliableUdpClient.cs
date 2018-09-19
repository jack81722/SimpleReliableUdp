using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp
{
    /// <summary>
    /// Reliable UDP client
    /// </summary>
    public sealed class ReliableUdpClient : UdpClient
    {
        /// <summary>
        /// Reliability controller
        /// </summary>
        private ReliabilityController ReliabilityController;

        #region Constructor
        public ReliableUdpClient() : base()
        {
            ReliabilityController = new ReliabilityController(this);
        }

        public ReliableUdpClient(AddressFamily family) : base(family)
        {
            ReliabilityController = new ReliabilityController(this);
        }

        public ReliableUdpClient(int port) : base(port)
        {
            ReliabilityController = new ReliabilityController(this);
        }

        public ReliableUdpClient(IPEndPoint localEP) : base(localEP)
        {
            ReliabilityController = new ReliabilityController(this);
        }

        public ReliableUdpClient(int port, AddressFamily family) : base(port, family)
        {
            ReliabilityController = new ReliabilityController(this);
        }

        public ReliableUdpClient(string hostname, int port) : base(hostname, port)
        {
            ReliabilityController = new ReliabilityController(this);
        }
        #endregion

        #region Send
        public new void Send(byte[] dgram, int bytes)
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, false);
            base.Send(dgram, bytes);
            //ReliabilityController.ReliableSend(dgram, bytes, false);
        }

        public new void Send(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            if (Client.Connected)
                throw new InvalidOperationException("Cannot send packets to an arbitrary host while connected.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, false);
            base.Send(dgram, bytes, endPoint);
            //ReliabilityController.ReliableSend(dgram, bytes, endPoint, false);
        }

        public new Task<int> SendAsync(byte[] dgram, int bytes)
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, false);
            return base.SendAsync(dgram, bytes);
            //return ReliabilityController.ReliableSendAsync(dgram, bytes, false);
        }

        public new Task<int> SendAsync(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            if (Client.Connected)
                throw new InvalidOperationException("Cannot send packets to an arbitrary host while connected.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, false);
            return base.SendAsync(dgram, bytes, endPoint);
            //return ReliabilityController.ReliableSendAsync(dgram, bytes, endPoint, false);
        }

        public new IAsyncResult BeginSend(byte[] dgram, int bytes, AsyncCallback requestCallback, object state)
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, false);
            return base.BeginSend(dgram, bytes, requestCallback, state);
        }

        public new IAsyncResult BeginSend(byte[] dgram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
        {
            if (Client.Connected)
                throw new InvalidOperationException("Cannot send packets to an arbitrary host while connected.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, false);
            return base.BeginSend(dgram, bytes, endPoint, requestCallback, state);
        }

        public void Send(byte[] dgram, int bytes, bool reliable)
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, reliable);
            base.Send(dgram, bytes);
            //ReliabilityController.ReliableSend(dgram, bytes, reliable);
        }

        public void Send(byte[] dgram, int bytes, IPEndPoint endPoint, bool reliable)
        {
            if (Client.Connected)
                throw new InvalidOperationException("Cannot send packets to an arbitrary host while connected.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, reliable);
            base.Send(dgram, bytes, endPoint);
            //ReliabilityController.ReliableSend(dgram, bytes, endPoint, reliable);
        }

        public Task<int> SendAsync(byte[] dgram, int bytes, bool reliable)
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, reliable);
            return base.SendAsync(dgram, bytes);
            //return ReliabilityController.ReliableSendAsync(dgram, bytes, reliable);
        }

        public Task<int> SendAsync(byte[] dgram, int bytes, IPEndPoint endPoint, bool reliable)
        {
            if (Client.Connected)
                throw new InvalidOperationException("Cannot send packets to an arbitrary host while connected.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, reliable);
            return base.SendAsync(dgram, bytes, endPoint);
            //return ReliabilityController.ReliableSendAsync(dgram, bytes, endPoint, reliable);
        }

        public IAsyncResult BeginSend(byte[] dgram, int bytes, AsyncCallback requestCallback, object state, bool reliable)
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, reliable);
            return base.BeginSend(dgram, bytes, requestCallback, state);
        }

        public IAsyncResult BeginSend(byte[] dgram, int bytes, IPEndPoint endPoint,AsyncCallback requestCallback, object state, bool reliable)
        {
            if (Client.Connected)
                throw new InvalidOperationException("Cannot send packets to an arbitrary host while connected.");
            ReliabilityController.AddReliable(ref dgram, ref bytes, reliable);
            return base.BeginSend(dgram, bytes, endPoint, requestCallback, state);
        }
        #endregion

        #region Receive
        public new byte[] Receive(ref IPEndPoint endPoint)
        {
            return ReliabilityController.RealizableReceive(ref endPoint);
        }

        public new async Task<UdpReceiveResult> ReceiveAsync()
        {
            if (!Client.Connected)
                throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
            return await ReliabilityController.RealizableReceiveAsync();
        }

        //AsyncCallback recvCallback;
        //object recvState;
        //public new IAsyncResult BeginReceive(AsyncCallback requestCallback, object state)
        //{
        //    recvCallback = new AsyncCallback(requestCallback);
        //    recvState = state;
        //    return base.BeginReceive(recvCallback, state);
        //}

        //public new byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint endPoint)
        //{
        //    if (Client.Connected)
        //        throw new InvalidOperationException("The operation is not allowed on non - connected sockets.");
        //    return ReliabilityController.ReliableEndReceive(recvCallback, recvState, asyncResult, ref endPoint);
        //}

        

        #endregion
    }
}
