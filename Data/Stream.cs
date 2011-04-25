using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A byte stream that can be read from.
    /// </summary>
    /// <remarks>Usually, an InStream has a limited size, but due to performance considerations,
    /// requesting data an InStream doesn't have leads to undefined results. At low levels, it can
    /// be assumed that an InStream has an infinite size and all data it returns is valid. The Data class is
    /// more robust and can be used to check the size and validity of data.</remarks>
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
        /// Advances the stream by the given amount of bytes.
        /// </summary>
        public void Advance(int Amount)
        {
            this.Advance((long)Amount);
        }

        /// <summary>
        /// Advances the stream by the given amount of bytes.
        /// </summary>
        public virtual void Advance(long Amount)
        {
            for (long t = 0; t < Amount; t++)
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
        /// Reads a boolean value from the source stream.
        /// </summary>
        public bool ReadBool()
        {
            return this.Read() != 0;
        }

        /// <summary>
        /// Reads a byte from the source stream.
        /// </summary>
        public byte ReadByte()
        {
            return this.Read();
        }

        /// <summary>
        /// Reads a 32-bit integer from the source stream.
        /// </summary>
        public int ReadInt()
        {
            return
                this.Read() |
                this.Read() << 8 |
                this.Read() << 16 |
                this.Read() << 24;
        }

        /// <summary>
        /// Reads a 64-bit integer from the source stream.
        /// </summary>
        public long ReadLong()
        {
            return
                this.Read() |
                this.Read() << 8 |
                this.Read() << 16 |
                this.Read() << 24 |
                this.Read() << 32 |
                this.Read() << 40 |
                this.Read() << 48 |
                this.Read() << 56;
        }

        /// <summary>
        /// Reads a bigint with the given size in bytes.
        /// </summary>
        public BigInt ReadBigInt(int Size)
        {
            uint[] digs = new uint[(Size + 3) / 4];
            int cur = 0;
            while (Size > 0)
            {
                if (Size >= 4)
                {
                    digs[cur] =
                        (uint)this.Read() |
                        (uint)this.Read() << 8 |
                        (uint)this.Read() << 16 |
                        (uint)this.Read() << 24;
                    cur++;
                    Size -= 4;
                    continue;
                }
                if (Size == 3)
                {
                    digs[cur] =
                        (uint)this.Read() |
                        (uint)this.Read() << 8 |
                        (uint)this.Read() << 16;
                    break;
                }
                if (Size == 2)
                {
                    digs[cur] =
                        (uint)this.Read() |
                        (uint)this.Read() << 8;
                    break;
                }
                if (Size == 1)
                {
                    digs[cur] =
                        (uint)this.Read();
                    break;
                }
            }
            return new BigInt(digs);
        }

        /// <summary>
        /// Reads a bigint value from the stream.
        /// </summary>
        public BigInt ReadBigInt()
        {
            int size = this.ReadInt();
            return this.ReadBigInt(size);
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
        /// Copies data from a source stream into this stream.
        /// </summary>
        public virtual void Write(InStream Source, long Amount)
        {
            while (Amount > 0)
            {
                this.Write(Source.Read());
                Amount--;
            }
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

        /// <summary>
        /// Advances the stream by the given amount of bytes. The data that is written is undefined.
        /// </summary>
        public void Advance(int Amount)
        {
            this.Advance((long)Amount);
        }

        /// <summary>
        /// Advances the stream by the given amount of bytes. The data that is written is undefined.
        /// </summary>
        public virtual void Advance(long Amount)
        {
            for (long t = 0; t < Amount; t++)
            {
                this.Write(0);
            }
        }

        /// <summary>
        /// Called when writing is complete.
        /// </summary>
        public virtual void Finish()
        {

        }

        /// <summary>
        /// Writes a boolean value to the source stream.
        /// </summary>
        public void WriteBool(bool Value)
        {
            this.Write(Value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Writes a byte to the source stream.
        /// </summary>
        public void WriteByte(byte Value)
        {
            this.Write(Value);
        }

        /// <summary>
        /// Writes a 32-bit integer to the source stream.
        /// </summary>
        public void WriteInt(int Value)
        {
            this.Write((byte)(Value));
            this.Write((byte)(Value >> 8));
            this.Write((byte)(Value >> 16));
            this.Write((byte)(Value >> 24));
        }

        /// <summary>
        /// Writes a 64-bit integer to the source stream.
        /// </summary>
        public void WriteLong(long Value)
        {
            this.Write((byte)(Value));
            this.Write((byte)(Value >> 8));
            this.Write((byte)(Value >> 16));
            this.Write((byte)(Value >> 24));
            this.Write((byte)(Value >> 32));
            this.Write((byte)(Value >> 40));
            this.Write((byte)(Value >> 48));
            this.Write((byte)(Value >> 56));
        }

        /// <summary>
        /// Writes a bigint value to the stream, truncating the value to the given size in bytes.
        /// </summary>
        public void WriteBigInt(BigInt Value, int Size)
        {
            for (int t = 0; t < Value.Digits.Length; t++)
            {
                uint dig = Value.Digits[t];
                if (Size >= 4)
                {
                    this.Write((byte)(dig));
                    this.Write((byte)(dig >> 8));
                    this.Write((byte)(dig >> 16));
                    this.Write((byte)(dig >> 24));
                    Size -= 4;
                    continue;
                }
                if (Size == 3)
                {
                    this.Write((byte)(dig));
                    this.Write((byte)(dig >> 8));
                    this.Write((byte)(dig >> 16));
                    break;
                }
                if (Size == 2)
                {
                    this.Write((byte)(dig));
                    this.Write((byte)(dig >> 8));
                    break;
                }
                if (Size == 1)
                {
                    this.Write((byte)(dig));
                    break;
                }
                break;
            }
        }

        /// <summary>
        /// Writes a bigint value to the stream.
        /// </summary>
        public void WriteBigInt(BigInt Value)
        {
            int bs = Value.ByteSize;
            this.WriteInt(bs);
            this.WriteBigInt(Value, bs);
        }
    }

    /// <summary>
    /// A stream that does not store written values, but instead counts the amount of bytes that have been written.
    /// </summary>
    public class CounterOutStream : OutStream
    {
        public CounterOutStream()
        {

        }

        public CounterOutStream(long Initial)
        {
            this._Count = Initial;
        }

        public override void Write(byte Data)
        {
            this._Count++;
        }

        public override void Write(byte[] Buffer, int Offset, int Length)
        {
            this._Count += Length;
        }

        public override void Write(InStream Source, long Amount)
        {
            this._Count += Amount;
            Source.Advance(Amount);
        }

        /// <summary>
        /// Gets or sets the current byte count for the stream.
        /// </summary>
        public long Count
        {
            get
            {
                return this._Count;
            }
            set
            {
                this._Count = value;
            }
        }

        private long _Count;
    }
}