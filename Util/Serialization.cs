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
        void Serialize(T Object, OutStream.F Stream);

        /// <summary>
        /// Deserializes an object from the stream.
        /// </summary>
        T Deserialize(InStream.F Stream);

        /// <summary>
        /// Gets the length in bytes of serialized objects, or nothing if the serialization does not
        /// have a fixed size. Note that in order to insure that the stream does output this many bytes, Flush
        /// should be called before and after serializing and deserializing the object.
        /// </summary>
        Maybe<int> Length { get; }
    }
}