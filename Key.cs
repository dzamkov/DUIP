using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Variable-length key for cryptographic use.
    /// </summary>
    public struct Key
    {
        public Key(byte[] Data)
        {
            this.Data = Data;
        }

        /// <summary>
        /// Gets the size in bytes of the key.
        /// </summary>
        public int Size
        {
            get
            {
                return this.Data.Length;
            }
        }

        public byte[] Data;
    }
}