using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// The possible endianness for data.
    /// </summary>
    public enum Endian
    {
        Little,
        Big
    }

    /// <summary>
    /// Contains format information of a stream to allow reading or writing primitive types. Note that the data in a stream is preserved exactly regardless
    /// of the details of the format, as long as the format used for reading and writing is the same.
    /// </summary>
    public struct StreamFormat
    {
        /// <summary>
        /// The endianness of the stream.
        /// </summary>
        public Endian Endian;

        /// <summary>
        /// Gets wether the endian of this format is the native endian for the current machine.
        /// </summary>
        public bool NativeEndian
        {
            get
            {
                return BitConverter.IsLittleEndian ^ (Endian == Endian.Big);
            }
        }

        /// <summary>
        /// Gets a useful default stream format, for when you're too lazy to specify your own.
        /// </summary>
        public static readonly StreamFormat Default = new StreamFormat()
        {
            Endian = Endian.Little
        };
    }

    /// <summary>
    /// A byte stream that can be read from.
    /// </summary>
    public abstract class InStream
    {
        /// <summary>
        /// Reads a single byte from the stream.
        /// </summary>
        public abstract byte Read();

        /// <summary>
        /// Reads data to the given buffer.
        /// </summary>
        public virtual void Read(byte[] Buffer, int Offset, int Length)
        {
            for (int t = 0; t < Length; t++)
            {
                Buffer[Offset++] = this.Read();
            }
        }

        /// <summary>
        /// Gets the amount of bytes available to read from the stream.
        /// </summary>
        public abstract int BytesAvailable { get; }

        /// <summary>
        /// Advances the stream by the given amount of bytes.
        /// </summary>
        public virtual void Advance(int Amount)
        {
            for (int t = 0; t < Amount; t++)
            {
                this.Read();
            }
        }

        /// <summary>
        /// Called when reading is complete.
        /// </summary>
        public virtual void Finish()
        {

        }

        /// <summary>
        /// Gets a formatted stream based on this stream.
        /// </summary>
        public F Format(StreamFormat Format)
        {
            return new F(this, Format);
        }

        /// <summary>
        /// A formatted in stream that allows reading of primitive types.
        /// </summary>
        public class F
        {
            public F(InStream Source, StreamFormat Format)
            {
                this._Source = Source;
                this._NativeEndian = Format.NativeEndian;
            }

            /// <summary>
            /// Gets the underlying source stream.
            /// </summary>
            public InStream Source
            {
                get
                {
                    return this._Source;
                }
            }

            /// <summary>
            /// Reads a boolean value from the source stream.
            /// </summary>
            public bool ReadBool()
            {
                return this._Source.Read() != 0;
            }

            /// <summary>
            /// Reads a byte from the source stream.
            /// </summary>
            public byte ReadByte()
            {
                return this._Source.Read();
            }

            /// <summary>
            /// Reads a 32-bit integer from the source stream.
            /// </summary>
            public int ReadInt()
            {
                if (this._NativeEndian)
                {
                    return
                        this._Source.Read() |
                        this._Source.Read() << 8 |
                        this._Source.Read() << 16 |
                        this._Source.Read() << 24;
                }
                else
                {
                    return
                        this._Source.Read() << 24 |
                        this._Source.Read() << 16 |
                        this._Source.Read() << 8 |
                        this._Source.Read();
                }
            }

            /// <summary>
            /// Insures pre-fetched data from the stream is destroyed. Calls to this method should correspond to calls to Flush
            /// in the writing stream. This method should be called before directly accessing the source stream.
            /// </summary>
            public void Flush()
            {

            }

            private InStream _Source;
            private bool _NativeEndian;
        }
    }

    /// <summary>
    /// A byte stream that can be written to.
    /// </summary>
    public abstract class OutStream
    {
        /// <summary>
        /// Writes a single byte to the stream.
        /// </summary>
        public abstract void Write(byte Data);

        /// <summary>
        /// Writes data from the given buffer.
        /// </summary>
        public virtual void Write(byte[] Buffer, int Offset, int Length)
        {
            for (int t = 0; t < Length; t++)
            {
                this.Write(Buffer[Offset++]);
            }
        }

        /// <summary>
        /// Called when writing is complete.
        /// </summary>
        public virtual void Finish()
        {

        }

        /// <summary>
        /// Gets a formatted stream based on this stream.
        /// </summary>
        public F Format(StreamFormat Format)
        {
            return new F(this, Format);
        }

        /// <summary>
        /// A formatted out stream that allows writing of primitive types.
        /// </summary>
        public class F
        {
            public F(OutStream Source, StreamFormat Format)
            {
                this._Source = Source;
                this._NativeEndian = Format.NativeEndian;
            }

            /// <summary>
            /// Gets the underlying source stream.
            /// </summary>
            public OutStream Source
            {
                get
                {
                    return this._Source;
                }
            }

            /// <summary>
            /// Writes a boolean value to the source stream.
            /// </summary>
            public void WriteBool(bool Value)
            {
                this._Source.Write(Value ? (byte)1 : (byte)0);
            }

            /// <summary>
            /// Writes a byte to the source stream.
            /// </summary>
            public void WriteByte(byte Value)
            {
                this._Source.Write(Value);
            }

            /// <summary>
            /// Writes a 32-bit integer to the source stream.
            /// </summary>
            public void WriteInt(int Value)
            {
                if (this._NativeEndian)
                {
                    this._Source.Write((byte)(Value));
                    this._Source.Write((byte)(Value >> 8));
                    this._Source.Write((byte)(Value >> 16));
                    this._Source.Write((byte)(Value >> 24));
                }
                else
                {
                    this._Source.Write((byte)(Value >> 24));
                    this._Source.Write((byte)(Value >> 16));
                    this._Source.Write((byte)(Value >> 8));
                    this._Source.Write((byte)(Value));
                }
            }

            /// <summary>
            /// Insures data queued to be written to the source stream is written. Calls to this method should correspond to calls to Flush
            /// in the reading stream. This method should be called before directly accessing the source stream.
            /// </summary>
            public void Flush()
            {

            }

            private OutStream _Source;
            private bool _NativeEndian;
        }
    }
}