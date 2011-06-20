using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DUIP.Net
{
    /// <summary>
    /// A possible type of a network message.
    /// </summary>
    public enum MessageType : byte
    {

    }

    /// <summary>
    /// A message sent between peers in a network.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Gets a data representation of this message.
        /// </summary>
        public abstract Data Data { get; }
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