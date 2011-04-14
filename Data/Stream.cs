using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
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
        /// Reads a boolean value from the stream.
        /// </summary>
        public virtual bool ReadBool()
        {
            return this.Read() != 0;
        }

        /// <summary>
        /// Reads an int value from the stream.
        /// </summary>
        public virtual int ReadInt()
        {
            int a = this.Read();
            int b = this.Read() << 8;
            int c = this.Read() << 16;
            int d = this.Read() << 24;
            return a | b | c | d;
        }

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
        /// Writes a boolean value to the stream.
        /// </summary>
        public virtual void WriteBool(bool Data)
        {
            this.Write(Data ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Writes an int value to the stream.
        /// </summary>
        public virtual void WriteInt(int Value)
        {
            this.Write((byte)(Value));
            this.Write((byte)(Value >> 8));
            this.Write((byte)(Value >> 16));
            this.Write((byte)(Value >> 24));
        }

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
    }
}