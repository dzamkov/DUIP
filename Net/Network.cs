using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// An interface to a distributed network that can store and retrieve indexed data and find the results
    /// of computional queries.
    /// </summary>
    public class Network
    {
        public Network(UDP UDP)
        {
            this._UDP = UDP;
            UDP.Receive += new UDP.ReceiveRawPacketHandler(this._Receive);
            this._Peers = new Dictionary<IPAddress, Peer>();
        }

        /// <summary>
        /// Gets the UDP client for this network.
        /// </summary>
        public UDP UDP
        {
            get
            {
                return this._UDP;
            }
        }

        /// <summary>
        /// Opens a connection to a peer given by an end point. Only one peer per IP address may be connected. If the peer is unresponsive,
        /// the connection may be closed. If a connection can not be started, this method will return null.
        /// </summary>
        public Peer Connect(IPEndPoint EndPoint)
        {
            IPAddress addr = EndPoint.Address;
            if (!this._Peers.ContainsKey(addr))
            {
                Peer peer = new Peer(EndPoint);
                this._Peers[EndPoint.Address] = peer;
                return peer;
            }
            return null;
        }

        /// <summary>
        /// Updates the state of the network by the given amount of time in seconds.
        /// </summary>
        public void Update(double Time)
        {

        }

        /// <summary>
        /// Gets the currently-connected peers on this network.
        /// </summary>
        public IEnumerable<Peer> Peers
        {
            get
            {
                return this._Peers.Values;
            }
        }

        /// <summary>
        /// Handler for when a packet is received.
        /// </summary>
        private void _Receive(IPEndPoint From, byte[] Data)
        {
         
        }

        private UDP _UDP;
        private Dictionary<IPAddress, Peer> _Peers;
    }

    /// <summary>
    /// Information about a peer on a network.
    /// </summary>
    public class Peer
    {
        public Peer(IPEndPoint EndPoint, InTerminal InTerminal, OutTerminal OutTerminal)
        {
            this._EndPoint = EndPoint;
        }

        /// <summary>
        /// Gets the endpoint used to send messages to this peer.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get
            {
                return this._EndPoint;
            }
        }

        /// <summary>
        /// Gets the IP address for this peer.
        /// </summary>
        public IPAddress Address
        {
            get
            {
                return this._EndPoint.Address;
            }
        }

        /// <summary>
        /// Gets the favor of the peer, which acts as an estimate of the ability of the peer to respond to queries while maintaining
        /// (and requesting) a low level of network traffic.
        /// </summary>
        public double Favor
        {
            get
            {
                return this._Favor;
            }
        }
        
        /// <summary>
        /// Gets the terminal that handles incoming communications with the peer.
        /// </summary>
        public InTerminal InTerminal
        {
            get
            {
                return this._InTerminal;
            }
        }

        /// <summary>
        /// Gets the terminal that handles outgoing communications with the peer.
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
        private double _Favor;
    }

    /// <summary>
    /// Describes the reward given to a peer for successfully responding to a query. The bounty of a query lets
    /// peers know how important a query is, wether they should respond, and how timely the response must be.
    /// </summary>
    /// <remarks>Note that only the first peer to respond will get the bounty, as any other responses will become useless. When a peer issues
    /// a query with a high bounty, the amount of network traffic to respond to the query will usually be greater, causing the peer to incur
    /// a favor penalty with the responders.</remarks>
    public struct Bounty
    {
        /// <summary>
        /// The base reward (in favor) of the bounty. If a peer immediately (or preemptively) responds to the query, its favor will
        /// increase by this amount.
        /// </summary>
        public double Base;

        /// <summary>
        /// The portion of the reward that remains after each second. This determines how time-sensitive the query is.
        /// </summary>
        public double Decay;
    }
}