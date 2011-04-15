using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A data source that can be read with a stream. The data can be assumed to be immutable, and can have multiple streams reading it at once.
    /// </summary>
    public abstract class Data
    {
        /// <summary>
        /// Creates a stream to read the data.
        /// </summary>
        public abstract InStream Read { get; }

        /// <summary>
        /// Gets the length, in bytes, of the data.
        /// </summary>
        public virtual int Length
        {
            get
            {
                return this.Read.BytesAvailable;
            }
        }
    }
}