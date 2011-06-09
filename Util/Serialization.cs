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
        /// Serializes an object to a stream. An exception may be thrown if there is a stream error.
        /// </summary>
        void Serialize(T Object, OutStream Stream);

        /// <summary>
        /// Deserializes an object from the stream. An exception may be thrown if there is a stream error, or no object could
        /// be derived from the stream.
        /// </summary>
        T Deserialize(InStream Stream);

        /// <summary>
        /// Gets the length in bytes of this serializable object for all values, or nothing if the size is variable or unbounded.
        /// </summary>
        Maybe<long> Size { get; }
    }

    /// <summary>
    /// An exception that is thrown when there is an attempt to deserialize an object from a stream that does not contain a
    /// valid object for the serialization method.
    /// </summary>
    public class DeserializationException : Exception
    {

    }

    /// <summary>
    /// Serialization method for primitive types.
    /// </summary>
    public class PrimitiveSerialization : 
        ISerialization<bool>,
        ISerialization<byte>,
        ISerialization<int>,
        ISerialization<uint>,
        ISerialization<long>,
        ISerialization<ulong>
    {
        void ISerialization<bool>.Serialize(bool Object, OutStream Stream)
        {
            Stream.WriteBool(Object);
        }

        bool ISerialization<bool>.Deserialize(InStream Stream)
        {
            return Stream.ReadBool();
        }

        Maybe<long> ISerialization<bool>.Size
        {
            get
            {
                return StreamSize.Bool;
            }
        }

        void ISerialization<byte>.Serialize(byte Object, OutStream Stream)
        {
            Stream.WriteByte(Object);
        }

        byte ISerialization<byte>.Deserialize(InStream Stream)
        {
            return Stream.ReadByte();
        }

        Maybe<long> ISerialization<byte>.Size
        {
            get
            {
                return StreamSize.Byte;
            }
        }

        void ISerialization<int>.Serialize(int Object, OutStream Stream)
        {
            Stream.WriteInt(Object);
        }

        int ISerialization<int>.Deserialize(InStream Stream)
        {
            return Stream.ReadInt();
        }

        Maybe<long> ISerialization<int>.Size
        {
            get
            {
                return StreamSize.Int;
            }
        }

        void ISerialization<uint>.Serialize(uint Object, OutStream Stream)
        {
            Stream.WriteInt((int)Object);
        }

        uint ISerialization<uint>.Deserialize(InStream Stream)
        {
            return (uint)Stream.ReadInt();
        }

        Maybe<long> ISerialization<uint>.Size
        {
            get
            {
                return StreamSize.Int;
            }
        }

        void ISerialization<long>.Serialize(long Object, OutStream Stream)
        {
            Stream.WriteLong(Object);
        }

        long ISerialization<long>.Deserialize(InStream Stream)
        {
            return Stream.ReadLong();
        }

        Maybe<long> ISerialization<long>.Size
        {
            get
            {
                return StreamSize.Long;
            }
        }

        void ISerialization<ulong>.Serialize(ulong Object, OutStream Stream)
        {
            Stream.WriteLong((long)Object);
        }

        ulong ISerialization<ulong>.Deserialize(InStream Stream)
        {
            return (ulong)Stream.ReadLong();
        }

        Maybe<long> ISerialization<ulong>.Size
        {
            get
            {
                return StreamSize.Long;
            }
        }
    }
}