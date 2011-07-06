using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP
{
    /// <summary>
    /// Describes a method of reading and writing an object of a certain type to a stream.
    /// </summary>
    /// <typeparam name="T">The common base type for serializable objects.</typeparam>
    public interface ISerialization<T>
    {
        /// <summary>
        /// Writes an object to a stream. An exception may be thrown if there is a stream error.
        /// </summary>
        void Write(T Object, OutStream Stream);

        /// <summary>
        /// Reads an object from the stream. An exception may be thrown if there is a stream error, or no object could
        /// be derived from the stream.
        /// </summary>
        T Read(InStream Stream);

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
    /// A serialization method for a constant value.
    /// </summary>
    public class ConstantSerialization<T> : ISerialization<T>
    {
        public ConstantSerialization(T Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// Gets the constant value that may be serialized.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public void Write(T Object, OutStream Stream)
        {

        }

        public T Read(InStream Stream)
        {
            return this._Value;
        }

        public Maybe<long> Size
        {
            get
            {
                return 0;
            }
        }

        private T _Value;
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
        void ISerialization<bool>.Write(bool Object, OutStream Stream)
        {
            Stream.WriteBool(Object);
        }

        bool ISerialization<bool>.Read(InStream Stream)
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

        void ISerialization<byte>.Write(byte Object, OutStream Stream)
        {
            Stream.WriteByte(Object);
        }

        byte ISerialization<byte>.Read(InStream Stream)
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

        void ISerialization<int>.Write(int Object, OutStream Stream)
        {
            Stream.WriteInt(Object);
        }

        int ISerialization<int>.Read(InStream Stream)
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

        void ISerialization<uint>.Write(uint Object, OutStream Stream)
        {
            Stream.WriteInt((int)Object);
        }

        uint ISerialization<uint>.Read(InStream Stream)
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

        void ISerialization<long>.Write(long Object, OutStream Stream)
        {
            Stream.WriteLong(Object);
        }

        long ISerialization<long>.Read(InStream Stream)
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

        void ISerialization<ulong>.Write(ulong Object, OutStream Stream)
        {
            Stream.WriteLong((long)Object);
        }

        ulong ISerialization<ulong>.Read(InStream Stream)
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