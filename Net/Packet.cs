using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Net
{
    /// <summary>
    /// A possible format for a packet to use.
    /// </summary>
    public enum PacketType : byte
    {
        /// <summary>
        /// The packet contains the entirety of a message.
        /// </summary>
        Complete,

        /// <summary>
        /// The packet contains part of a message.
        /// </summary>
        Partial,

        /// <summary>
        /// The packet invalidates a previously-sent message.
        /// </summary>
        Cancel,

        /// <summary>
        /// The packet is a request for a message, or certain parts of a message.
        /// </summary>
        Request,

        /// <summary>
        /// The packet tests if the connection is valid, and gives the initial message number used by the terminal.
        /// </summary>
        Handshake,

        /// <summary>
        /// The packet informs the receiver that the connection has been closed and no more message may be sent or received.
        /// </summary>
        Disconnect
    }

}