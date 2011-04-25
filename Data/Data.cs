using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A data source that can be read with a stream.
    /// </summary>
    public abstract class Data
    {
        /// <summary>
        /// Creates a stream to read the data or returns null if not possible.
        /// </summary>
        public abstract InStream Read();

        /// <summary>
        /// Creates a stream to read this data, starting at the given position.
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
        /// Gets the size, in bytes, of the data.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// Creates a stream to modify the data beginning at the given position. The modifing stream may not go over the
        /// bounds of the data. Null is returned if the data can not be modified.
        /// </summary>
        public virtual OutStream Modify(long Start)
        {
            return null;
        }

        /// <summary>
        /// Creates a stream to modify the data from the beginning.
        /// </summary>
        public OutStream Modify()
        {
            return Modify(0);
        }

        /// <summary>
        /// Gets if this data is immutable. Immutable data can not be modified using the "Modify" method or by some external method.
        /// </summary>
        public virtual bool Immutable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a partion of this data.
        /// </summary>-
        public PartionData Partion(long Start, long Size)
        {
            return new PartionData(this, Start, Size);
        }
    }

    /// <summary>
    /// A data representation of a contiguous subset of some other data.
    /// </summary>
    public class PartionData : Data
    {
        public PartionData(Data Source, long Start, long Size)
        {
            this._Source = Source;
            this._Start = Start;
            this._Size = Size;
        }

        /// <summary>
        /// Gets the source data for this partion.
        /// </summary>
        public Data Source
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

        public override bool Immutable
        {
            get
            {
                return this._Source.Immutable;
            }
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

        private Data _Source;
        private long _Start;
        private long _Size;
    }
}