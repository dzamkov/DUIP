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
                    if (carry == 0)
                    {
                        return;
                    }
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
        /// Subtracts a BigInt in-place from this value. Any carried values will be ignored.
        /// </summary>
        /// <param name="Offset">The offset in this BigInt's digits to start adding to. The given Int will, in effect, be multipled
        /// by (base ^ Offset) before subtracting.</param>
        public void Subtract(BigInt Int, int Offset)
        {
            int cur = 0;
            uint carry = 0;
            while (Offset < this.Digits.Length)
            {
                if (cur < Int.Digits.Length)
                {
                    uint ncarry;
                    Subtract(this.Digits[Offset], carry, out this.Digits[Offset], out ncarry);
                    Subtract(this.Digits[Offset], Int.Digits[cur], out this.Digits[Offset], out carry);
                    carry += ncarry;
                    cur++; Offset++;
                }
                else
                {
                    Subtract(this.Digits[Offset], carry, out this.Digits[Offset], out carry);
                    if (carry > 0)
                    {
                        Offset++;
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Subtracts a BigInt in-place from this value. Any carried values will be ignored.
        /// </summary>
        public void Subtract(BigInt Int)
        {
            this.Subtract(Int, 0);
        }

        /// <summary>
        /// Multiplies the value in-place by a certain multiplier.
        /// </summary>
        public void Multiply(uint Multiplier)
        {
            uint carry = 0;
            for(int offset = 0; offset < this.Digits.Length; offset++)
            {
                uint acarry, bcarry;
                Multiply(this.Digits[offset], Multiplier, out this.Digits[offset], out acarry);
                Add(this.Digits[offset], carry, out this.Digits[offset], out bcarry);
                carry = acarry + bcarry;
            }
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
        /// Subtracts B from A. If B is greater than A, the result will be the 2's complement form
        /// of the negative integer result.
        /// </summary>
        public static BigInt Subtract(BigInt A, BigInt B)
        {
            BigInt res = BigInt.Empty(Math.Max(A.Digits.Length, B.Digits.Length) + 1);
            res.Copy(A);
            res.Subtract(B);
            return res;
        }

        /// <summary>
        /// Multiplies two integers together.
        /// </summary>
        public static BigInt Multiply(BigInt A, BigInt B)
        {
            BigInt res = BigInt.Empty(A.Digits.Length + B.Digits.Length);
            BigInt temp = BigInt.Empty(A.Digits.Length + 1);
            for (int t = 0; t < B.Digits.Length; t++)
            {
                temp.Copy(A);
                temp.Multiply(B.Digits[t]);
                res.Add(temp, t);
            }
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
        /// Adds together two integers, giving the result and the carry value. Note that the carry value is to be
        /// subtracted from the next highest digit.
        /// </summary>
        public static void Subtract(uint A, uint B, out uint Result, out uint Carry)
        {
            ulong asrc = (ulong)A;
            ulong bsrc = (ulong)B;
            ulong lres = asrc - bsrc;
            Result = (uint)lres;
            Carry = (uint)-(int)(lres >> 32);
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

        public static BigInt operator -(BigInt A, BigInt B)
        {
            return Subtract(A, B);
        }

        public static BigInt operator *(BigInt A, BigInt B)
        {
            return Multiply(A, B);
        }

        /// <summary>
        /// The digits, in little-endian order, for the integer.
        /// </summary>
        public uint[] Digits;
    }
}