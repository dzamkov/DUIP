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
        }

        private UDP _UDP;
    }

    /// <summary>
    /// Information about a peer on a network.
    /// </summary>
    public class Peer
    {
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

        private IPEndPoint _EndPoint;
    }
}