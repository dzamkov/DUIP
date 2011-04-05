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
        /// Determines wether some content is an instance of the given type.
        /// </summary>
        public abstract Query<bool> IsInstance(Type Type, Content Content);

        /// <summary>
        /// Serializes an instance of a certain type to a stream.
        /// </summary>
        public abstract void Serialize(Type Type, Content Instance, OutByteStream Stream);

        /// <summary>
        /// Deserializes an instance of a certain type from a stream, or returns null if not possible.
        /// </summary>
        public abstract Query<Content> Deserialize(Type Type, InByteStream Stream);
    }
}