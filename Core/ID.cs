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

        public override string ToString()
        {
            return
                this.A.ToString("{0:X2}") + "-" +
                this.B.ToString("{0:X2}") + "-" +
                this.C.ToString("{0:X2}") + "-" +
                this.D.ToString("{0:X2}");
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
    public class IDType : Type<ID>
    {
        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IDType Singleton = new IDType();

        public override void Serialize(Context Context, ID Instance, OutStream Stream)
        {
            Stream.WriteInt(Instance.A);
            Stream.WriteInt(Instance.B);
            Stream.WriteInt(Instance.C);
            Stream.WriteInt(Instance.D);
        }

        public override ID Deserialize(Context Context, InStream Stream)
        {
            return new ID(
                Stream.ReadInt(),
                Stream.ReadInt(),
                Stream.ReadInt(),
                Stream.ReadInt());
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.ID);
        }
    }
}