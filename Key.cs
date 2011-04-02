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

        /// <summary>
        /// Gets a portion of this key.
        /// </summary>
        public Key Sub(int Start, int Size)
        {
            byte[] d = new byte[Size];
            for (int t = 0; t < Size; t++)
            {
                d[t] = this.Data[Start++];
            }
            return d;
        }

        /// <summary>
        /// Concats two keys.
        /// </summary>
        public static Key operator +(Key A, Key B)
        {
            byte[] td = new byte[A.Size + B.Size];
            int i = 0;
            for (int t = 0; t < A.Size; t++)
            {
                td[i++] = A.Data[t];
            }
            for (int t = 0; t < B.Size; t++)
            {
                td[i++] = B.Data[t];
            }
            return td;
        }

        public static implicit operator Key(byte[] Data)
        {
            return new Key(Data);
        }

        public static implicit operator byte[](Key Key)
        {
            return Key.Data;
        }

        public byte[] Data;
    }
}