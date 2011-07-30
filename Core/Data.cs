using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A finite, uninterpreted sequence of bytes.
    /// </summary>
    public abstract class Data
    {
        /// <summary>
        /// Gets the size in bytes of the data.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// Gets the value of a byte in this data.
        /// </summary>
        public byte this[long Position]
        {
            get
            {
                byte val;
                using (Disposable<InStream> str = this.Read(Position))
                {
                    val = str.Object.ReadByte();
                }
                return val;
            }
        }

        /// <summary>
        /// Gets a stream to read this data. There may be any number of open streams for a single data object at one time.
        /// </summary>
        public Disposable<InStream> Read()
        {
            return this.Read(0);
        }

        /// <summary>
        /// Gets a stream to read this data beginning at the given position.
        /// </summary>
        public abstract Disposable<InStream> Read(long Start);

        /// <summary>
        /// Gets a partion of this data.
        /// </summary>
        public virtual Data GetPartion(long Start, long Size)
        {
            return new PartionData(this, Start, Size);
        }

        /// <summary>
        /// Gets a region of this data. The resulting data will have a size smaller than requested if the bounds of the region exceeds
        /// the size of the data.
        /// </summary>
        public Data GetRegion(DataRegion Region)
        {
            long dsize = this.Size;
            long size = Math.Max(0, Math.Min(Region.Size, dsize - Region.Start));
            return this.GetPartion(Region.Start, size);
        }

        /// <summary>
        /// Gets data from a buffer.
        /// </summary>
        public static PartionData FromBuffer(byte[] Buffer, long Offset, long Size)
        {
            return new PartionData(new BufferData(Buffer), Offset, Size);
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

        public static implicit operator byte[](Data Data)
        {
            long size = Data.Size;
            int isize = (int)size;
            if ((long)isize >= size)
            {
                byte[] buffer = new byte[isize];
                using (Disposable<InStream> str = Data.Read())
                {
                    str.Object.Read(buffer, 0, isize);
                }
                return buffer;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static ConcatData operator +(Data Initial, Data Final)
        {
            long isize = Initial.Size;
            long fsize = Final.Size;
            return new ConcatData(
                new Data[] { Initial, Final },
                new long[] { 0, isize },
                isize + fsize);
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

        /// <summary>
        /// Creates data from the concatenation of parts.
        /// </summary>
        public static ConcatData Concat(Data[] Parts)
        {
            long[] offsets = new long[Parts.Length];
            long size = 0;
            for (int t = 0; t < Parts.Length; t++)
            {
                offsets[t] = size;
                size += Parts[t].Size;
            }
            return new ConcatData(Parts, offsets, size);
        }

        /// <summary>
        /// Breaks this data into chunks (given as partion data) with the given maximum size.
        /// </summary>
        public IEnumerable<Data> Break(long ChunkSize)
        {
            long size = this.Size;
            long i = 0;
            while (size > 0)
            {
                yield return this.GetPartion(i, Math.Min(ChunkSize, size));
                size -= ChunkSize;
                i += ChunkSize;
            }
        }
    }

    /// <summary>
    /// Specifies a region in data.
    /// </summary>
    public struct DataRegion
    {
        public DataRegion(long Start, long Size)
        {
            this.Start = Start;
            this.Size = Size;
        }

        /// <summary>
        /// A data region that covers the entirety of any data it is applied to.
        /// </summary>
        public static readonly DataRegion Full = new DataRegion
        {
            Start = 0,
            Size = long.MaxValue
        };

        /// <summary>
        /// Writes a region to a stream.
        /// </summary>
        public static void Write(DataRegion Region, OutStream Stream)
        {
            Stream.WriteLong(Region.Start);
            Stream.WriteLong(Region.Size);
        }

        /// <summary>
        /// Reads a region from a stream.
        /// </summary>
        public static DataRegion Read(InStream Stream)
        {
            return new DataRegion
            {
                Start = Stream.ReadLong(),
                Size = Stream.ReadLong()
            };
        }

        /// <summary>
        /// The position of the first byte in the region.
        /// </summary>
        public long Start;

        /// <summary>
        /// The maximum size of the region.
        /// </summary>
        public long Size;
    }

    /// <summary>
    /// Data with 0 bytes.
    /// </summary>
    public class NullData : Data
    {
        private NullData()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly NullData Instance = new NullData();

        public override Disposable<InStream> Read(long Start)
        {
            return null;
        }

        public override long Size
        {
            get
            {
                return 0;
            }
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

        public override Disposable<InStream> Read(long Start)
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

        public override Disposable<InStream> Read(long Start)
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

        public override Disposable<InStream> Read(long Start)
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
        public class Stream : InStream, IDisposable
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
                    ((Disposable<InStream>)this._LocalStream).Dispose();
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
                    ((Disposable<InStream>)this._LocalStream).Dispose();
                    Data next = this._Parts[++this._LocalPart];
                    this._LocalStream = next.Read();
                    this._LocalStreamSize = next.Size;
                }
            }

            public void Dispose()
            {
                ((Disposable<InStream>)this._LocalStream).Dispose();
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
}