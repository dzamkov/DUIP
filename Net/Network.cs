using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interface to an external network.
    /// </summary>
    /// <typeparam name="TRef">Reference to a datum in the network.</typeparam>
    public abstract class Network<T>
    {
        /// <summary>
        /// Gets the credential used in the network.
        /// </summary>
        public abstract Credential Credential { get; }

        /// <summary>
        /// Looks for the datum with the specified index in the network.
        /// </summary>
        public abstract Query<Datum> this[T Index] { get; }

        /// <summary>
        /// Serializes a datum reference (not the actual datum) in this network to a byte stream.
        /// </summary>
        public abstract void Serialize(T Ref, OutByteStream Stream);

        /// <summary>
        /// Deserializes a datum reference from a byte stream.
        /// </summary>
        public abstract T Deserialize(InByteStream Stream);
    }
}