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

        public ID(byte[] Seed)
        {
            this.A = BitConverter.ToInt32(Seed, 0);
            this.B = BitConverter.ToInt32(Seed, 4);
            this.C = BitConverter.ToInt32(Seed, 8);
            this.D = BitConverter.ToInt32(Seed, 12);
        }

        static ID()
        {
            TypeID = Serialize.CreateTypeID("Core/ID");
            Serialize.RegisterDeserializer(TypeID, new DeserializeHandler(Deserialize));
        }

        void Serializable.Serialize(BinaryWriteStream Target)
        {
            Target.WriteInt(this.A);
            Target.WriteInt(this.B);
            Target.WriteInt(this.C);
            Target.WriteInt(this.D);
        }

        ID Serializable.TypeID
        {
            get
            {
                return TypeID;
            }
        }

        public static Serializable Deserialize(BinaryReadStream Stream)
        {
            return new ID(Stream.ReadInt(), Stream.ReadInt(), Stream.ReadInt(), Stream.ReadInt());
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
            MD5 al = MD5.Create();
            byte[] input = Encoding.ASCII.GetBytes(String);
            byte[] hash = al.ComputeHash(input);
            return new ID(hash);
        }

        public override string ToString()
        {
            return
                String.Format("{0:X2}", this.A) + "-" + 
                String.Format("{0:X2}", this.B) + "-" +
                String.Format("{0:X2}", this.C) + "-" +
                String.Format("{0:X2}", this.D);
        }

        public Int32 A;
        public Int32 B;
        public Int32 C;
        public Int32 D;
        private static Random PNGR;
        public static readonly ID TypeID;
    }
}
