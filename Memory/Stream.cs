using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A byte stream that can be read from.
    /// </summary>
    /// <remarks>Stream functions may throw an exception to indicate stream errors (such as out of memory, or end of stream).</remarks>
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
            while(--Length >= 0)
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
        /// Reads a boolean value from the stream.
        /// </summary>
        public bool ReadBool()
        {
            return this.Read() != 0;
        }

        /// <summary>
        /// Reads a byte from the stream.
        /// </summary>
        public byte ReadByte()
        {
            return this.Read();
        }

        /// <summary>
        /// Reads a 32-bit integer from the stream.
        /// </summary>
        public int ReadInt()
        {
            byte[] buf = new byte[4];
            this.Read(buf, 0, 4);
            int val = BitConverter.ToInt32(buf, 0);
            if (!BitConverter.IsLittleEndian)
            {
                Endian.Swap(ref val);
            }
            return val;
        }

        /// <summary>
        /// Reads a 64-bit integer from the stream.
        /// </summary>
        public long ReadLong()
        {
            byte[] buf = new byte[8];
            this.Read(buf, 0, 8);
            long val = BitConverter.ToInt64(buf, 0);
            if (!BitConverter.IsLittleEndian)
            {
                Endian.Swap(ref val);
            }
            return val;
        }

        /// <summary>
        /// Reads a 64-bit floating point number from the stream.
        /// </summary>
        public double ReadDouble()
        {
            long lval = this.ReadLong();
            return BitConverter.Int64BitsToDouble(lval);
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
    /// Exception thrown when a read stream does not have enough data available to fufill a read operation.
    /// </summary>
    public class StreamUnderflowException : Exception
    {

    }

    /// <summary>
    /// Contains the sizes of primitives in bytes.
    /// </summary>
    public static class StreamSize
    {
        public const int Bool = 1;
        public const int Byte = 1;
        public const int Int = 4;
        public const int Long = 8;
        public const int Double = 8;
    }

    /// <summary>
    /// A byte stream that can be written to.
    /// </summary>
    /// <remarks>Stream functions may throw an exception to indicate stream errors (such as out of memory, or end of stream).</remarks>
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
        /// Writes a boolean value to the stream.
        /// </summary>
        public void WriteBool(bool Value)
        {
            this.Write(Value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        public void WriteByte(byte Value)
        {
            this.Write(Value);
        }

        /// <summary>
        /// Writes a 32-bit integer to the stream.
        /// </summary>
        public void WriteInt(int Value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Endian.Swap(ref Value);
            }
            this.Write(BitConverter.GetBytes(Value), 0, 4);
        }

        /// <summary>
        /// Writes a 64-bit integer to the stream.
        /// </summary>
        public void WriteLong(long Value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Endian.Swap(ref Value);
            }
            this.Write(BitConverter.GetBytes(Value), 0, 8);
        }

        /// <summary>
        /// Writes a 64-bit floating point number to the stream.
        /// </summary>
        public void WriteDouble(double Value)
        {
            long lval = BitConverter.DoubleToInt64Bits(Value);
            this.WriteLong(lval);
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

    /// <summary>
    /// A stream that reads from a buffer (an array of bytes).
    /// </summary>
    public sealed class BufferInStream : InStream
    {
        public BufferInStream(byte[] Buffer, long Position)
        {
            this._Buffer = Buffer;
            this._Position = Position;
        }

        /// <summary>
        /// Gets the buffer this stream is reading from.
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                return this._Buffer;
            }
        }

        /// <summary>
        /// Gets the current position of this stream within the buffer.
        /// </summary>
        public long Position
        {
            get
            {
                return this._Position;
            }
        }

        public override byte Read()
        {
            return this._Buffer[this._Position++];
        }

        public override void Read(byte[] Buffer, int Offset, int Length)
        {
            while (--Length >= 0)
            {
                Buffer[Offset++] = this._Buffer[this._Position++];
            }
        }

        public override void Advance(long Amount)
        {
            this._Position += Amount;
        }

        private byte[] _Buffer;
        private long _Position;
    }

    /// <summary>
    /// A stream that writes to a buffer (an array of bytes).
    /// </summary>
    public sealed class BufferOutStream : OutStream
    {
        public BufferOutStream(byte[] Buffer, long Position)
        {
            this._Buffer = Buffer;
            this._Position = Position;
        }

        /// <summary>
        /// Gets the buffer this stream is reading from.
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                return this._Buffer;
            }
        }

        /// <summary>
        /// Gets the current position of this stream within the buffer.
        /// </summary>
        public long Position
        {
            get
            {
                return this._Position;
            }
        }

        public override void Write(byte Data)
        {
            this._Buffer[this._Position++] = Data;
        }

        public override void Write(byte[] Buffer, int Offset, int Length)
        {
            while (Length-- > 0)
            {
                this._Buffer[this._Position++] = Buffer[Offset++];
            }
        }

        public override void Advance(long Amount)
        {
            this._Position += Amount;
        }

        private byte[] _Buffer;
        private long _Position;
    }
}