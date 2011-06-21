using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A network consisting of peers connected through a common UDP interface.
    /// </summary>
    public class UDPNetwork : Network
    {
        public UDPNetwork(UDP UDP)
        {
            this._UDP = UDP;
            UDP.Receive += new UDP.ReceiveRawPacketHandler(this._Receive);
            this._Peers = new Dictionary<IPEndPoint, UDPPeer>();
        }

        public override IEnumerable<Peer> Peers
        {
            get
            {
                return this._Peers.Values.Cast<Peer>();
            }
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
        /// Updates the state of the network by the given amount of time in seconds, sending packets if needed.
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

        public override event Action<Peer, Message> Receive;

        private UDP _UDP;
        private Dictionary<IPEndPoint, UDPPeer> _Peers;
    }

    /// <summary>
    /// A peer on a UDP network.
    /// </summary>
    public class UDPPeer : Peer
    {
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

        private IPEndPoint _EndPoint;
        private InTerminal _InTerminal;
        private OutTerminal _OutTerminal;
    }
}