using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An arbitrarily-large positive integer.
    /// </summary>
    public struct BigInt
    {
        public BigInt(uint Value)
        {
            this.Digits = new uint[] { Value };
        }

        public BigInt(uint[] Digits)
        {
            this.Digits = Digits;
        }

        /// <summary>
        /// Gets the big-int representation of zero.
        /// </summary>
        public static BigInt Zero
        {
            get
            {
                return new BigInt() { Digits = new uint[0] };
            }
        }

        /// <summary>
        /// Creates a BigInt with a value of zero and the specified amount of digits.
        /// </summary>
        public static BigInt Empty(int Size)
        {
            return new BigInt(new uint[Size]);
        }

        /// <summary>
        /// Adds a BigInt in-place to this value. Any carried values will be ignored.
        /// </summary>
        /// <param name="Offset">The offset in this BigInt's digits to start adding to. The given Int will, in effect, be multipled
        /// by (base ^ Offset) before adding.</param>
        public void Add(BigInt Int, int Offset)
        {
            int cur = 0;
            uint carry = 0;
            while (Offset < this.Digits.Length)
            {
                if (cur < Int.Digits.Length)
                {
                    uint ncarry;
                    Add(this.Digits[Offset], carry, out this.Digits[Offset], out ncarry);
                    Add(this.Digits[Offset], Int.Digits[cur], out this.Digits[Offset], out carry);
                    carry += ncarry;
                    cur++; Offset++;
                }
                else
                {
                    Add(this.Digits[Offset], carry, out this.Digits[Offset], out carry);
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a BigInt in-place to this value. Any carried values will be ignored.
        /// </summary>
        public void Add(BigInt Int)
        {
            this.Add(Int, 0);
        }

        /// <summary>
        /// Copies a BigInt in-place to this value, starting at the given offset in this value's digits.
        /// </summary>
        public void Copy(BigInt Int, int Offset)
        {
            int cur = 0;
            while (Offset < this.Digits.Length && cur < Int.Digits.Length)
            {
                this.Digits[Offset] = Int.Digits[cur];
                cur++; Offset++;
            }
        }

        /// <summary>
        /// Copies a BigInt in-place to this value.
        /// </summary>
        public void Copy(BigInt Int)
        {
            this.Copy(Int, 0);
        }

        /// <summary>
        /// Adds two integers together.
        /// </summary>
        public static BigInt Add(BigInt A, BigInt B)
        {
            BigInt res = BigInt.Empty(Math.Max(A.Digits.Length, B.Digits.Length) + 1);
            res.Copy(A);
            res.Add(B);
            return res;
        }

        /// <summary>
        /// Adds together two integers, giving the result and the carry value.
        /// </summary>
        public static void Add(uint A, uint B, out uint Result, out uint Carry)
        {
            ulong asrc = (ulong)A;
            ulong bsrc = (ulong)B;
            ulong lres = asrc + bsrc;
            Result = (uint)lres;
            Carry = (uint)(lres >> 32);
        }

        /// <summary>
        /// Multiplies together two integers, giving the result and the carry value.
        /// </summary>
        public static void Multiply(uint A, uint B, out uint Result, out uint Carry)
        {
            ulong asrc = (ulong)A;
            ulong bsrc = (ulong)B;
            ulong lres = asrc * bsrc;
            Result = (uint)lres;
            Carry = (uint)(lres >> 32);
        }

        public static implicit operator BigInt(uint Int)
        {
            return new BigInt(Int);
        }

        public static BigInt operator +(BigInt A, BigInt B)
        {
            return Add(A, B);
        }


        /// <summary>
        /// The digits, in little-endian order, for the integer.
        /// </summary>
        public uint[] Digits;
    }
}