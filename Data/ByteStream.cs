using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A byte stream that can be read from.
    /// </summary>
    public abstract class InByteStream
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
    public abstract class OutByteStream
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
    }
}