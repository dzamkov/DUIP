using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents a finite, uninterpreted sequence of bytes.
    /// </summary>
    public abstract class Data
    {
        /// <summary>
        /// Gets the size in bytes of the data.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// Gets a stream to read this data. There may be any number of open streams for a single data object at one time.
        /// </summary>
        public abstract InStream Read();

        /// <summary>
        /// Gets a stream to read this data beginning at the given position.
        /// </summary>
        public virtual InStream Read(long Start)
        {
            InStream stream = this.Read();
            stream.Advance(Start);
            return stream;
        }

        /// <summary>
        /// Gets a partion of this data.
        /// </summary>
        public virtual Data GetPartion(long Start, long Size)
        {
            return new PartionData(this, Start, Size);
        }

        /// <summary>
        /// Gets if the two data are equivalent in content.
        /// </summary>
        public static bool Equal(Data A, Data B)
        {
            long size = A.Size;
            if (size != B.Size)
            {
                return false;
            }
            InStream astr = A.Read();
            InStream bstr = B.Read();
            while (size > 0)
            {
                if (astr.Read() != bstr.Read())
                {
                    return false;
                }
                size--;
            }
            return true;
        }
    }

    /// <summary>
    /// Data from a byte array.
    /// </summary>
    public sealed class BufferData : Data
    {
        public BufferData(byte[] Buffer)
        {
            this._Buffer = Buffer;
        }

        /// <summary>
        /// Gets the buffer for this data. The buffer should not be modified.
        /// </summary>
        public byte[] Buffer
        {
            get
            {
                return this._Buffer;
            }
        }

        public override InStream Read()
        {
            return new BufferInStream(this._Buffer, 0);
        }

        public override InStream Read(long Start)
        {
            return new BufferInStream(this._Buffer, Start);
        }

        public override long Size
        {
            get
            {
                return this._Buffer.LongLength;
            }
        }

        private byte[] _Buffer;
    }

    /// <summary>
    /// Data from a contiguous subset of some other data. 
    /// </summary>
    public sealed class PartionData : Data
    {
        public PartionData(Data Source, long Start, long Size)
        {
            this._Source = Source;
            this._Start = Start;
            this._Size = Size;
        }

        /// <summary>
        /// Gets the source data of this partion.
        /// </summary>
        public Data Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the position of the beginning of this partion in the source data.
        /// </summary>
        public long Start
        {
            get
            {
                return this._Start;
            }
        }

        public override long Size
        {
            get
            {
                return this._Size;
            }
        }

        public override InStream Read()
        {
            return this._Source.Read(this._Start);
        }

        public override InStream Read(long Start)
        {
            return this._Source.Read(this._Start + Start);
        }

        public override Data GetPartion(long Start, long Size)
        {
            return new PartionData(this._Source, this._Start + Start, this._Size);
        }

        private Data _Source;
        private long _Start;
        private long _Size;
    }

    /// <summary>
    /// A type for data.
    /// </summary>
    public class DataType : Type
    {
        private DataType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly DataType Singleton = new DataType();

        public override bool Equal(object A, object B)
        {
            return DUIP.Data.Equal(A as Data, B as Data);
        }
    }
}