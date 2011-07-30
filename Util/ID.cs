using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A 128-bit identifier.
    /// </summary>
    public struct ID
    {
        public ID(int A, int B, int C, int D)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        /// <summary>
        /// Gets the null (blank) id.
        /// </summary>
        public static ID Null
        {
            get
            {
                return new ID(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Gets a hashing method for ID's.
        /// </summary>
        public static IHashing<ID> Hashing
        {
            get
            {
                return IDHashing.Singleton;
            }
        }

        public override string ToString()
        {
            return
                this.A.ToString("X8") + "-" +
                this.B.ToString("X8") + "-" +
                this.C.ToString("X8") + "-" +
                this.D.ToString("X8");
        }

        /// <summary>
        /// Gets the relation between two ID's.
        /// </summary>
        public static Relation Compare(ID A, ID B)
        {
            if (A.A < B.A)
                return Relation.Lesser;
            if (A.A > B.A)
                return Relation.Greater;
            if (A.B < B.B)
                return Relation.Lesser;
            if (A.B > B.B)
                return Relation.Greater;
            if (A.C < B.C)
                return Relation.Lesser;
            if (A.C > B.C)
                return Relation.Greater;
            if (A.D < B.D)
                return Relation.Lesser;
            if (A.D > B.D)
                return Relation.Greater;
            return Relation.Equal;
        }

        /// <summary>
        /// Gets if two ID's are equal.
        /// </summary>
        public static bool Equal(ID A, ID B)
        {
            return A.A == B.A && A.B == B.B && A.C == B.C && A.D == B.D;
        }

        /// <summary>
        /// Writes an ID to a stream.
        /// </summary>
        public static void Write(ID ID, OutStream Stream)
        {
            Stream.WriteInt(ID.A);
            Stream.WriteInt(ID.B);
            Stream.WriteInt(ID.C);
            Stream.WriteInt(ID.D);
        }

        /// <summary>
        /// Reads an ID from a stream.
        /// </summary>
        public static ID Read(InStream Stream)
        {
            return new ID
            {
                A = Stream.ReadInt(),
                B = Stream.ReadInt(),
                C = Stream.ReadInt(),
                D = Stream.ReadInt()
            };
        }

        public override int GetHashCode()
        {
            return this.A ^ this.B ^ this.C ^ this.D;
        }

        public override bool Equals(object obj)
        {
            ID? id = obj as ID?;
            if (id != null)
            {
                ID i = id.Value;
                return Equal(this, i);
            }
            return false;
        }

        public int A;
        public int B;
        public int C;
        public int D;
    }

    /// <summary>
    /// A hashing method for ID's.
    /// </summary>
    public sealed class IDHashing : IHashing<ID>
    {
        private IDHashing()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IDHashing Singleton = new IDHashing();

        public BigInt Hash(ID Object)
        {
            return new BigInt(new uint[]
            {
                (uint)Object.A,
                (uint)Object.B,
                (uint)Object.C,
                (uint)Object.D
            });
        }

        public bool Equal(ID A, ID B)
        {
            return DUIP.ID.Equal(A, B);
        }
    }
}