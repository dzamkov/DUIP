using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interface to an external network.
    /// </summary>
    public abstract class Network : Context
    {
        /// <summary>
        /// Gets the credential used in the network.
        /// </summary>
        public abstract Credential Credential { get; }

        /// <summary>
        /// Looks for the datum, specified by the reference, in the network.
        /// </summary>
        public abstract Query<Datum> this[Reference Reference] { get; }

        /// <summary>
        /// Serializes a reference for this network to the given output stream.
        /// </summary>
        public abstract void Serialize(Reference Reference, OutStream Stream);

        /// <summary>
        /// Deserializes a reference for this network from a input stream.
        /// </summary>
        public abstract Reference Deserialize(InStream Stream);
    }
}