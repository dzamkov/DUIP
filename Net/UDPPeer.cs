﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A peer on a UDP network.
    /// </summary>
    public class UDPPeer : Peer
    {
        internal UDPPeer(IPEndPoint EndPoint, int SequenceNumber, int AcknowledgementNumber)
        {
            this._EndPoint = EndPoint;
            this._LastSequenceNumber = AcknowledgementNumber;
            this._InTerminal = new InTerminal(AcknowledgementNumber);
            this._OutTerminal = new OutTerminal(SequenceNumber);
        }

        /// <summary>
        /// Gets the size of the chunks to break messages into for sending.
        /// </summary>
        public const int ChunkSize = 1024;

        /// <summary>
        /// Gets the endpoint for this peer.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get
            {
                return this._EndPoint;
            }
        }

        /// <summary>
        /// Gets the terminal that handles incoming messages from this peer.
        /// </summary>
        public InTerminal InTerminal
        {
            get
            {
                return this._InTerminal;
            }
        }

        /// <summary>
        /// Gets the terminal that handles outgoing messages to this peer.
        /// </summary>
        public OutTerminal OutTerminal
        {
            get
            {
                return this._OutTerminal;
            }
        }

        public override void Send(Message Message)
        {
            this._OutTerminal.Send(Message.Write(), ChunkSize);
        }

        /// <summary>
        /// Processes a chunk of a message sent from this peer.
        /// </summary>
        public void Process(int SequenceNumber, Data Data, bool Initial, bool Final)
        {
            if (_After(SequenceNumber, this._LastSequenceNumber))
            {
                this._LastSequenceNumber = SequenceNumber;
            }

            Data d = null;
            if (this._InTerminal.Receive(SequenceNumber, Data, Initial, Final, ref d))
            {
                Message m = Message.Read(d);
                if (m != null && this.Receive != null)
                {
                    this.Receive(this, m);
                }
            }
        }

        /// <summary>
        /// Determines wether a packet with the given sequence number is likely to be valid (authentic).
        /// </summary>
        public bool Valid(int SequenceNumber)
        {
            return
                _After(SequenceNumber, this._InTerminal.AcknowledgementNumber) &&
                _After(this._LastSequenceNumber + _ValidThreshold, SequenceNumber);
        }

        /// <summary>
        /// The allowable difference between the last sequence number and the sequence number of the next received
        /// packet in order for the packet to be considered valid.
        /// </summary>
        private const int _ValidThreshold = 64;

        /// <summary>
        /// Determines wether the sequence number A is likely to occur after (or is) the sequence number B.
        /// </summary>
        private static bool _After(int A, int B)
        {
            return A >= B || A < (B ^ int.MinValue);
        }

        public override event Action<Peer, Message> Receive;

        private IPEndPoint _EndPoint;
        private InTerminal _InTerminal;
        private OutTerminal _OutTerminal;
        private int _LastSequenceNumber;
    }

    /// <summary>
    /// Manages a set of UDP peers that use a common UDP interface. Also allows new connections to be made.
    /// </summary>
    public class UDPHub
    {
        public UDPHub(UDP UDP)
        {
            this._UDP = UDP;
            UDP.Receive += new UDP.ReceiveRawPacketHandler(this._Receive);
            this._Peers = new Dictionary<IPEndPoint, UDPPeer>();
        }

        /// <summary>
        /// Tries connecting to a peer specified by an IP end point. If the peer is already connected, it will be immediately returned. If
        /// a connection can not be made, null will be returned (either immediately or after some period of time).
        /// </summary>
        public Query<UDPPeer> Connect(IPEndPoint EndPoint)
        {
            UDPPeer peer;
            if (this._Peers.TryGetValue(EndPoint, out peer))
            {
                return peer;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the state of the hub by the given amount of time in seconds, sending packets if needed.
        /// </summary>
        public void Update(double Time)
        {
            throw new NotImplementedException();
        }

        private void _Receive(IPEndPoint From, byte[] Packet)
        {
            // Find the peer that sent this message
            UDPPeer peer;
            if (this._Peers.TryGetValue(From, out peer))
            {
                throw new NotImplementedException();
            }
        }

        private UDP _UDP;
        private Dictionary<IPEndPoint, UDPPeer> _Peers;
    }
}