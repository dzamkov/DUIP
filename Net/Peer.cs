using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// Represents a connected peer that can send and receive network messages.
    /// </summary>
    public abstract class Peer
    {
        /// <summary>
        /// The favor of the peer, which acts as an estimate of the ability of the peer to respond to queries while maintaining
        /// (and requesting) a low level of network traffic.
        /// </summary>
        public double Favor;

        /// <summary>
        /// Sends a network message to this peer.
        /// </summary>
        public abstract void Send(Message Message);

        /// <summary>
        /// Gets the average amount of time, in seconds, it would take for messages to be sent both ways, to and
        /// from this peer.
        /// </summary>
        public abstract double RoundTripTime { get; }

        /// <summary>
        /// Event fired when a message is received from this peer.
        /// </summary>
        public abstract event Action<Peer, Message> Receive;

        /// <summary>
        /// Event fired when this peer has been disconnected and may no longer send or receive messages.
        /// </summary>
        public abstract event Action<Peer> Disconnect;
    }
}