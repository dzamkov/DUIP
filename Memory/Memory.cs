using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A data source that can be read or modified with streams.
    /// </summary>
    public abstract class Memory
    {
        /// <summary>
        /// Creates a stream to read the memory or returns null if not possible.
        /// </summary>
        public abstract InStream Read();

        /// <summary>
        /// Creates a stream to read this memory, starting at the given position.
        /// </summary>
        public virtual InStream Read(long Start)
        {
            InStream r = this.Read();
            if (r != null)
            {
                r.Advance(Start);
                return r;
            }
            return null;
        }

        /// <summary>
        /// Gets the size, in bytes, of the memory.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// Creates a stream to modify the memory beginning at the given position. The modifing stream may not go over the
        /// bounds of the memory. Null is returned if the data can not be modified.
        /// </summary>
        public virtual OutStream Modify(long Start)
        {
            return null;
        }

        /// <summary>
        /// Creates a stream to modify the memory from the beginning.
        /// </summary>
        public OutStream Modify()
        {
            return Modify(0);
        }

        /// <summary>
        /// Gets a partion of this memory.
        /// </summary>-
        public PartionMemory GetPartion(long Start, long Size)
        {
            return new PartionMemory(this, Start, Size);
        }

        /// <summary>
        /// Gets a data from a section of this memory in its current state, or returns null if not possible. The resulting data must be immutable and all
        /// read calls on the data must succeed.
        /// </summary>
        public virtual Data GetData(long Start, long Size)
        {
            InStream str = this.Read(Start);
            if (str != null)
            {
                // Creating a buffer is the only way to insure data can not be modified.
                try
                {
                    int isize = checked((int)Size);
                    byte[] buffer = new byte[isize];
                    str.Read(buffer, 0, isize);
                    return new BufferData(buffer);
                }
                catch (OverflowException)
                {
                    return null;
                }
                finally
                {
                    str.Finish();
                }
            }
            return null;
        }
    }

    /// <summary>
    /// A memory representation of a contiguous subset of some other memory.
    /// </summary>
    public class PartionMemory : Memory
    {
        public PartionMemory(Memory Source, long Start, long Size)
        {
            this._Source = Source;
            this._Start = Start;
            this._Size = Size;
        }

        /// <summary>
        /// Gets the source data for this partion.
        /// </summary>
        public Memory Source
        {
            get
            {
                return this._Source;
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

        public override long Size
        {
            get
            {
                return this._Size;
            }
        }

        public override OutStream Modify(long Start)
        {
            return this._Source.Modify(this._Start + Start);
        }

        public override Data GetData(long Start, long Size)
        {
            return this._Source.GetData(this._Start + Start, Size);
        }

        private Memory _Source;
        private long _Start;
        private long _Size;
    }
}