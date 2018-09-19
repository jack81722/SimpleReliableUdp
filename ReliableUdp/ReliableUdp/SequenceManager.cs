using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ReliableUdp
{
    /// <summary>
    /// Manager of sequence number and resend events
    /// </summary>
    public class SequenceManager
    {
        #region Properties
        /// <summary>
        /// Time out (ms)
        /// </summary>
        public const int timeout = 30;

        /// <summary>
        /// Sequence number of packet (0~255)
        /// </summary>
        public byte sequenceNum { get; private set; }

        /// <summary>
        /// Temp of packet for resending
        /// </summary>
        private Dictionary<byte, byte[]> packetTemp;

        /// <summary>
        /// Timer of resending
        /// </summary>
        private Dictionary<byte, Timer> resendTimers;

        /// <summary>
        /// Statistics of resend
        /// </summary>
        private Dictionary<byte, int> resendStatistics;
        #endregion

        #region Constructor
        public SequenceManager()
        {
            sequenceNum = 0;
            packetTemp = new Dictionary<byte, byte[]>();
            resendTimers = new Dictionary<byte, Timer>();
            resendStatistics = new Dictionary<byte, int>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Register resend event
        /// </summary>
        /// <param name="dgram">packet dgram</param>
        /// <param name="resend">resend action</param>
        public void RegisterResendEvent(byte[] dgram, Action<byte> resend)
        {
            byte seqNum = sequenceNum;
            // Check if sequence number is not using
            if (packetTemp.ContainsKey(seqNum))
            {
                throw new Exception("Exceed maximum sequence number, check if the network is not congestion.");
            }
            
            packetTemp.Add(seqNum, dgram);          // Save packet temporary 
            Timer timer = new Timer(timeout);       // Create new timer for resend
            timer.Elapsed += (obj, args) =>         // Register resend event to timer
            {
                resend.Invoke(seqNum);
            };
            timer.AutoReset = false;                // Set timer don't auto reset
            resendTimers.Add(seqNum, timer);        // Save timer
            resendStatistics.Add(seqNum, 0);        // Save statistic of resend count
            sequenceNum++;                          // Next sequence

            timer.Start();                          // Start timer
        }

        /// <summary>
        /// Remove sequence event by sequence number
        /// </summary>
        /// <param name="seqNum">sequence number of data dgram</param>
        public void RemoveResendEvent(byte seqNum)
        {
            packetTemp.Remove(seqNum);              // Remove packet temporary by sequence number
            resendTimers[seqNum].Stop();            // Stop timer of resend event
            resendTimers[seqNum].Dispose();         // Dispose timer of resend event
            resendTimers.Remove(seqNum);            // Remove timer
            resendStatistics.Remove(seqNum);        // Remove statistic of resend count
        }

        /// <summary>
        /// Get resend data dgram by sequence number
        /// </summary>
        /// <param name="seqNum">sequence number of data dgram</param>
        public byte[] GetResendData(byte seqNum)
        {
            byte[] dgram = packetTemp[seqNum];      // Get dgram of packet by sequence number
            resendStatistics[seqNum]++;             // Add resend count because of resend
            resendTimers[seqNum].Start();           // Restart timer
            return dgram;
        }
        #endregion
    }
}
