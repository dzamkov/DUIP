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
        /// Gets a serialization method for an ID.
        /// </summary>
        public static ISerialization<ID> Serialization
        {
            get
            {
                return IDType.Singleton;
            }
        }

        /// <summary>
        /// Gets an ordering method for ID's.
        /// </summary>
        public static IOrdering<ID> Ordering
        {
            get
            {
                return IDType.Singleton;
            }
        }

        public override string ToString()
        {
            return
                this.A.ToString("{0:X2}") + "-" +
                this.B.ToString("{0:X2}") + "-" +
                this.C.ToString("{0:X2}") + "-" +
                this.D.ToString("{0:X2}");
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
                return this.A == i.A && this.B == i.B && this.C == i.C && this.D == i.D;
            }
            return false;
        }

        public int A;
        public int B;
        public int C;
        public int D;
    }

    /// <summary>
    /// A type for an id.
    /// </summary>
    public class IDType : Type<ID>, ISerialization<ID>, IOrdering<ID>
    {
        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IDType Singleton = new IDType();

        public void Serialize(ID Object, OutStream.F Stream)
        {
            Stream.WriteInt(Object.A);
            Stream.WriteInt(Object.B);
            Stream.WriteInt(Object.C);
            Stream.WriteInt(Object.D);
        }

        public ID Deserialize(InStream.F Stream)
        {
            return new ID(
                Stream.ReadInt(),
                Stream.ReadInt(),
                Stream.ReadInt(),
                Stream.ReadInt());
        }

        Maybe<int> ISerialization<ID>.Length
        {
            get
            {
                return 4 * 4;
            }
        }

        public Relation Compare(ID A, ID B)
        {
            return ID.Compare(A, B);
        }
    }
}