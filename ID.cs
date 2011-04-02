using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A unique identifier for content.
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
}