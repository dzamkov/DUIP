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
        /// The packet contains raw network data.
        /// </summary>
        /// <remarks>This is similar to using a packet of type "Content" with a content type of "Data", but it
        /// does not require the ContentType or the size of the data to be explicitly specified</remarks>
        Raw,

        /// <summary>
        /// The packet contains a single instance of network content.
        /// </summary>
        Content,

        /// <summary>
        /// The packet contains multiple instance of network content.
        /// </summary>
        MultiContent,
    }

    /// <summary>
    /// A possible format of network content.
    /// </summary>
    public enum ContentType : byte
    {
        /// <summary>
        /// The content defines raw network data. The data may later be identified by its hash.
        /// </summary>
        Raw,

        /// <summary>
        /// The content defines data by concating recently-defined network data (using their identification hashes). The defined data
        /// will be identified by a hash of the identifiers of the concated data (as opposed to the resulting data).
        /// </summary>
        Concat,

        /// <summary>
        /// The content requests the receiver to process and interpret the data associated with the given identification hash as a network
        /// command.
        /// </summary>
        Interpret,
    }


}