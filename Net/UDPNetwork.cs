using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A network consisting of peers connected through a common UDP interface.
    /// </summary>
    public class UDPNetwork
    {
        public UDPNetwork(UDP UDP)
        {
            this._UDP = UDP;
            UDP.Receive += new UDP.ReceiveRawPacketHandler(this._Receive);
            this._Peers = new Dictionary<IPEndPoint, UDPPeer>();
        }

        private void _Receive(IPEndPoint From, byte[] Packet)
        {

        }

        private UDP _UDP;
        private Dictionary<IPEndPoint, UDPPeer> _Peers;
    }

    /// <summary>
    /// A peer on a UDP network.
    /// </summary>
    public class UDPPeer : Peer
    {
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

        private IPEndPoint _EndPoint;
        private InTerminal _InTerminal;
        private OutTerminal _OutTerminal;
    }
}