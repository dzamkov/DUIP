//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DUIP.Core
{
    /// <summary>
    /// Global identifier for all sorts of data.
    /// </summary>
    public struct ID : Serializable
    {
        public ID(Int32 A, Int32 B, Int32 C, Int32 D)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public ID(BinaryReadStream Stream)
        {
            this.A = Stream.ReadInt();
            this.B = Stream.ReadInt();
            this.C = Stream.ReadInt();
            this.D = Stream.ReadInt();
        }

        public ID(byte[] Seed) : this(new ByteArrayReader(Seed))
        {
            
        }

        /// <summary>
        /// Converts this id to a byte array.
        /// </summary>
        /// <returns>A 16 byte array representing this ID.</returns>
        public byte[] ToByteArray()
        {
            ByteArrayWriter baw = new ByteArrayWriter();
            this.Serialize(baw);
            return baw.Data;
        }

        public void Serialize(BinaryWriteStream Target)
        {
            Target.WriteInt(this.A);
            Target.WriteInt(this.B);
            Target.WriteInt(this.C);
            Target.WriteInt(this.D);
        }

        /// <summary>
        /// Creates a random id. This should be used when creating a
        /// id based on data can't be done.
        /// </summary>
        /// <returns>A random id.</returns>
        public static ID Random()
        {
            if (PNGR == null)
            {
                PNGR = new Random();
            }
            byte[] seed = new byte[16];
            PNGR.NextBytes(seed);
            return new ID(seed);
        }

        /// <summary>
        /// Creates an id from the hash of a string. The id will always be the same for the specified
        /// string.
        /// </summary>
        /// <param name="String">The string to hash.</param>
        /// <returns>An id based on the hash of the specified string.</returns>
        public static ID Hash(string String)
        {
            return Hash(Encoding.ASCII.GetBytes(String));
        }

        /// <summary>
        /// Creates an id that is the hash of the specified data.
        /// </summary>
        /// <param name="Data">The input data.</param>
        /// <returns>An id based on the hash of the input data.</returns>
        public static ID Hash(byte[] Data)
        {
            MD5 al = MD5.Create();
            return new ID(al.ComputeHash(Data));
        }

        /// <summary>
        /// Creates an iterative id that is based on another id.
        /// </summary>
        /// <param name="ID">The id to use to create the next id.</param>
        /// <returns>The next id.</returns>
        public static ID Hash(ID ID)
        {
            return Hash(ID.ToByteArray());
        }

        /// <summary>
        /// Hashes an array of id's. The resulting hash will be based
        /// on the permutation of input id's.
        /// </summary>
        /// <param name="IDs">The array of input id's.</param>
        /// <returns>The hashed value of the input id's.</returns>
        public static ID Hash(IEnumerable<ID> IDs)
        {
            ByteArrayWriter baw = new ByteArrayWriter();
            foreach (ID i in IDs)
            {
                i.Serialize(baw);
            }
            return Hash(baw.Data);
        }

        public override string ToString()
        {
            return
                String.Format("{0:X2}", this.A) + "-" + 
                String.Format("{0:X2}", this.B) + "-" +
                String.Format("{0:X2}", this.C) + "-" +
                String.Format("{0:X2}", this.D);
        }

        public int A; // Do not use these values directly,
        public int B; // they may have strange endians.
        public int C;
        public int D;
        private static Random PNGR;
    }
}
