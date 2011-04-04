using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents a range of contents called instances and properties common to all instances.
    /// </summary>
    public abstract class Type : Content
    {
        
    }

    /// <summary>
    /// A version of a type specific to a network.
    /// </summary>
    public abstract class NetworkType<T>
    {
        /// <summary>
        /// Checks if the given content is an instance of this type.
        /// </summary>
        public abstract Query<bool> IsInstance(Content Content);

        /// <summary>
        /// Serializes a known instance of this type to the given stream.
        /// </summary>
        public abstract void Serialize(Content Instance, OutByteStream Stream);

        /// <summary>
        /// Deserializes an instance of this type from a stream, or returns null if not possible.
        /// </summary>
        public abstract Query<Content> Deserialize(InByteStream Stream);

        /// <summary>
        /// Gets the network this type is for.
        /// </summary>
        public abstract Network<T> Network { get; }
    }
}