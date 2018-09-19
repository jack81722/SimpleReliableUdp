using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp
{
    /// <summary>
    /// Process reliability options and events
    /// </summary>
    public class ReliabilityController
    {
        #region Realizable Parameters
        /// <summary>
        /// Reliable flags
        /// </summary>
        public enum ReliableFlag : byte
        {
            Realizable = 128,              
            Seq = 64,
            Ack = 32,
        }

        /// <summary>
        /// The udp client of reliability controller
        /// </summary>
        private ReliableUdpClient udpClient;

        /// <summary>
        /// Resend manager of reliability controller
        /// </summary>
        private SequenceManager resendManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of reliability controller
        /// </summary>
        public ReliabilityController(ReliableUdpClient udpClient)
        {   
            this.udpClient = udpClient;
            resendManager = new SequenceManager();
        }
        #endregion

        #region RealizableSend
        /// <summary>
        /// Add reliable header to dgram
        /// </summary>
        /// <param name="dgram">packet dgram</param>
        /// <param name="bytes">packet dgram length</param>
        /// <param name="reliable">reliable boolean</param>
        public void AddReliable(ref byte[] dgram, ref int bytes, bool reliable)
        {
            if (reliable)
            {
                dgram = AddReliableHeader(dgram, (byte)(ReliableFlag.Realizable | ReliableFlag.Seq), resendManager.sequenceNum);
                bytes += 2;
                StartResend(dgram);
            }
            else
            {
                byte[] reliableData = new byte[dgram.Length + 1];
                reliableData[0] = 0;
                Array.Copy(dgram, 0, reliableData, 1, dgram.Length);
                dgram = reliableData;
                bytes++;
            }
        }
        #endregion

        #region ReliableRecv
        /// <summary>
        /// Receive packet reliably
        /// </summary>
        /// <param name="endPoint">packet source end point</param>
        public byte[] RealizableReceive(ref IPEndPoint endPoint)
        {
            while (true)
            {
                byte[] result = ((UdpClient)udpClient).Receive(ref endPoint);
                byte[] buffer;
                if (DetectReliableHeader(result, endPoint, out buffer))
                {
                    continue;
                }
                return buffer;
            }
        }

        /// <summary>
        /// Async receive packet reliably
        /// </summary>
        public async Task<UdpReceiveResult> RealizableReceiveAsync()
        {
            while (true)
            {
                UdpReceiveResult result = await ((UdpClient)udpClient).ReceiveAsync();
                byte[] buffer;
                // ignore ack packet
                if (DetectReliableHeader(result.Buffer, result.RemoteEndPoint, out buffer))
                {
                    continue;
                }
                return new UdpReceiveResult(buffer, result.RemoteEndPoint);
            }
        }
        

        //public byte[] ReliableEndReceive(AsyncCallback requestCallback, object state, IAsyncResult asyncResult, ref IPEndPoint endPoint)
        //{
        //    while (true)
        //    {
        //        var dgram = ((UdpClient)udpClient).EndReceive(asyncResult, ref endPoint);
        //        byte[] buffer;
        //        if (DetectReliableHeader(dgram, endPoint, out buffer))
        //        {
        //            ((UdpClient)udpClient).BeginReceive(requestCallback, state);
        //            continue;
        //        }
        //        else
        //        {
        //            requestCallback(asyncResult);
        //        }
        //        return buffer;
        //    }
        //}


        #endregion

        #region RealizableMethod
        /// <summary>
        /// Start resend timer
        /// </summary>
        /// <param name="dgram">resend packet dgram</param>
        public void StartResend(byte[] dgram)
        {
            resendManager.RegisterResendEvent(dgram, (x) => Resend(x));
        }

        /// <summary>
        /// Start resend timer for specific end point
        /// </summary>
        /// <param name="dgram">resend packet dgram</param>
        /// <param name="endPoint">target end point</param>
        public void StartResend(byte[] dgram, IPEndPoint endPoint)
        {
            resendManager.RegisterResendEvent(dgram, (x) => Resend(x, endPoint));
        }

        /// <summary>
        /// Add reliable header to packet
        /// </summary>
        /// <param name="dgram">packet data</param>
        /// <param name="flags">reliable flags</param>
        /// <param name="seqNum">sequence number</param>
        public byte[] AddReliableHeader(byte[] dgram, byte flags, byte seqNum)
        {
            byte[] realizableData = new byte[dgram.Length + 2];
            realizableData[0] = flags;                                      // add realizable flag
            realizableData[1] = seqNum;
            Array.Copy(dgram, 0, realizableData, 2, dgram.Length);

            return realizableData;
        }

        /// <summary>
        /// Add reliable header to packet
        /// </summary>
        /// <param name="dgram">packet data</param>
        /// <param name="endPoint">source end point</param>
        /// <param name="buffer">actual data buffer without reliable options</param>
        public bool DetectReliableHeader(byte[] dgram, IPEndPoint endPoint, out byte[] buffer)
        {
            if ((dgram[0] & (byte)ReliableFlag.Realizable) > 0)
            {
                buffer = new byte[dgram.Length - 2];
                Array.Copy(dgram, 2, buffer, 0, buffer.Length);
                if ((dgram[0] & (byte)ReliableFlag.Seq) > 0)
                {
                    // reply ack
                    if (udpClient.Client.Connected)
                        SendAck(dgram[1]);
                    else
                        SendAck(dgram[1], endPoint);
                    return false;
                }
                if ((dgram[0] & (byte)ReliableFlag.Ack) > 0)
                {
                    // remove resend event
                    resendManager.RemoveResendEvent(dgram[1]);
                    return true;
                }
            }
            else
            {
                buffer = new byte[dgram.Length - 1];
                Array.Copy(dgram, 1, buffer, 0, buffer.Length);
            }
            return false;
        }

        /// <summary>
        /// Send ack by sequence number
        /// </summary>
        /// <param name="seqNum">sequence number</param>
        private void SendAck(byte seqNum)
        {
            byte[] dgram = AddReliableHeader(new byte[0], (byte)(ReliableFlag.Realizable | ReliableFlag.Ack), seqNum);
            ((UdpClient)udpClient).Send(dgram, dgram.Length);
        }

        /// <summary>
        /// Send ack by sequence number
        /// </summary>
        /// <param name="seqNum">sequence number</param>
        /// <param name="endPoint">target end point</param>
        private void SendAck(byte seqNum, IPEndPoint endPoint)
        {
            byte[] dgram = AddReliableHeader(new byte[0], (byte)(ReliableFlag.Realizable | ReliableFlag.Ack), seqNum);
            ((UdpClient)udpClient).Send(dgram, dgram.Length, endPoint);
        }

        /// <summary>
        /// Resend action
        /// </summary>
        /// <param name="seqNum">sequence number</param>
        private void Resend(byte seqNum)
        {
            try
            {
                byte[] dgram = resendManager.GetResendData(seqNum);
                ((UdpClient)udpClient).Send(dgram, dgram.Length);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ":" + e.StackTrace);
            }
        }

        /// <summary>
        /// Resend action
        /// </summary>
        /// <param name="seqNum">sequence number</param>
        /// <param name="endPoint">target end point</param>
        private void Resend(byte seqNum, IPEndPoint endPoint)
        {
            try
            {
                byte[] dgram = resendManager.GetResendData(seqNum);
                ((UdpClient)udpClient).Send(dgram, dgram.Length, endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ":" + e.StackTrace);
            }
        }
        #endregion
    }
}
