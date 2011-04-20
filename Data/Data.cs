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
        public virtual InStream Read(ulong Start)
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
        /// Gets the length, in bytes, of the data.
        /// </summary>
        public virtual ulong Length
        {
            get
            {
                return this.Read().BytesAvailable;
            }
        }

        /// <summary>
        /// Creates a stream to modify the data beginning at the given position. The modifing stream may not go over the
        /// bounds of the data. Null is returned if the data can not be modified.
        /// </summary>
        public virtual OutStream Modify(ulong Start)
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
    }
}