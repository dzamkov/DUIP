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
        public InStream Read()
        {
            return this.Read(0);
        }

        /// <summary>
        /// Gets a stream to read this data beginning at the given position.
        /// </summary>
        public abstract InStream Read(long Start);

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

        public static implicit operator Data(byte[] Buffer)
        {
            return new BufferData(Buffer);
        }

        /// <summary>
        /// Creates data from the concatenation of parts.
        /// </summary>
        public static ConcatData Concat(List<Data> Parts)
        {
            Data[] parts = new Data[Parts.Count];
            long[] offsets = new long[parts.Length];
            long size = 0;
            for (int t = 0; t < parts.Length; t++)
            {
                offsets[t] = size;
                parts[t] = Parts[t];
                size += parts[t].Size;
            }
            return new ConcatData(parts, offsets, size);
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
    /// Data created from the concatenation of parts.
    /// </summary>
    public sealed class ConcatData : Data
    {
        public ConcatData(Data[] Parts, long[] Offsets, long Size)
        {
            this._Parts = Parts;
            this._Offsets = Offsets;
            this._Size = Size;
        }

        public override long Size
        {
            get
            {
                return this._Size;
            }
        }

        public override InStream Read(long Start)
        {
            int part;
            long offset = Start;
            if (Start == 0)
            {
                part = 0;
            }
            else
            {
                // Use a binary search to find the part to start on
                int l = 0;
                int h = this._Parts.Length;
                while (h > l + 1)
                {
                    int s = (l + h) / 2;
                    long off = this._Offsets[s];
                    if (Start < off)
                    {
                        h = s;
                    }
                    else
                    {
                        l = s;
                        offset = Start - off;
                    }
                }
                part = l;
            }

            Data data = this._Parts[part];
            long size = data.Size;
            return new Stream(this._Parts, part, size - offset, data.Read(offset));
        }

        /// <summary>
        /// A stream for concatenated data.
        /// </summary>
        public class Stream : InStream
        {
            public Stream(Data[] Parts, int LocalPart, long LocalStreamSize, InStream LocalStream)
            {
                this._Parts = Parts;
                this._LocalPart = LocalPart;
                this._LocalStreamSize = LocalStreamSize;
                this._LocalStream = LocalStream;
            }

            public override byte Read()
            {
                this._Feed();
                this._LocalStreamSize--;
                return this._LocalStream.Read();
            }

            public override void Advance(long Amount)
            {
                if (Amount <= this._LocalStreamSize)
                {
                    this._LocalStream.Advance(Amount);
                    this._LocalStreamSize -= Amount;
                }
                else
                {
                    this._LocalStream.Finish();
                    Data next;
                    long size;
                    while ((size = (next = this._Parts[++this._LocalPart]).Size) < Amount)
                    {
                        Amount -= size;
                    }
                    this._LocalStream = next.Read(Amount);
                    this._LocalStreamSize = size - Amount;
                }
            }

            /// <summary>
            /// Insures that the local stream has a size of at least 1.
            /// </summary>
            private void _Feed()
            {
                while (this._LocalStreamSize == 0)
                {
                    this._LocalStream.Finish();
                    Data next = this._Parts[++this._LocalPart];
                    this._LocalStream = next.Read();
                    this._LocalStreamSize = next.Size;
                }
            }

            private Data[] _Parts;
            private int _LocalPart;
            private long _LocalStreamSize;
            private InStream _LocalStream;
        }

        private Data[] _Parts;
        private long[] _Offsets;
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