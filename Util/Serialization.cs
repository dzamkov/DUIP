using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Describes a method of serializing and deserializing an object of a certain type to a stream.
    /// </summary>
    /// <typeparam name="T">The common base type for serializable objects.</typeparam>
    public interface ISerialization<T>
    {
        /// <summary>
        /// Serializes an object to a stream.
        /// </summary>
        void Serialize(T Object, OutStream Stream);

        /// <summary>
        /// Deserializes an object from the stream.
        /// </summary>
        T Deserialize(InStream Stream);

        /// <summary>
        /// Gets the upper bound of the length in bytes of this serializable object for all values, or nothing if the size is unbounded.
        /// </summary>
        Maybe<long> Size { get; }
    }
}