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
        /// Creates a stream to read this memory, starting at the given position or returns null if not possible.
        /// </summary>
        public virtual Disposable<InStream> Read(long Start)
        {
            return null;
        }

        /// <summary>
        /// Creates a stream to read the memory from the beginning or returns null if not possible.
        /// </summary>
        public Disposable<InStream> Read()
        {
            return Read(0);
        }

        /// <summary>
        /// Gets the size, in bytes, of the memory.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// Creates a stream to modify the memory starting at the given position or returns null if not possible. The modifing stream 
        /// may not go over the bounds of the memory.
        /// </summary>
        public virtual Disposable<OutStream> Write(long Start)
        {
            return null;
        }

        /// <summary>
        /// Creates a stream to modify the memory from the beginning or returns null if not possible.
        /// </summary>
        public Disposable<OutStream> Write()
        {
            return Write(0);
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
                    ((Disposable<InStream>)str).Dispose();
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

        public override Disposable<InStream> Read(long Start)
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

        public override Disposable<OutStream> Write(long Start)
        {
            return this._Source.Write(this._Start + Start);
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