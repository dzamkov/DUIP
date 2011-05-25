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
    }

    /// <summary>
    /// A type for data.
    /// </summary>
    public class DataType : Type<Data>
    {
        public override bool Equal(Data A, Data B)
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

        public override Maybe<Data> Merge(Data Base, Data A, Data B)
        {
            // Make no attempt to merge, or even equate data. The process is usually unnecessary.
            return Maybe<Data>.Nothing;
        }
    }
}