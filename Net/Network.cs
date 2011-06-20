using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// An interface to a collection of peers that can send and receive network messages.
    /// </summary>
    public abstract class Network
    {

    }

    /// <summary>
    /// Represents a connected peer on a network.
    /// </summary>
    public abstract class Peer
    {
        /// <summary>
        /// The favor of the peer, which acts as an estimate of the ability of the peer to respond to queries while maintaining
        /// (and requesting) a low level of network traffic.
        /// </summary>
        public double Favor;
    }
}