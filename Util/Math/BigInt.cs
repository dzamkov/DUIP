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
        /// Trims excess zero digits from the integer. Returns itself.
        /// </summary>
        public BigInt Reduce
        {
            get
            {
                int nl = 0;
                for (int t = this.Digits.Length - 1; t >= 0; t--)
                {
                    if (this.Digits[t] > 0)
                    {
                        nl = t + 1;
                        break;
                    }
                }
                if (nl < this.Digits.Length)
                {
                    uint[] ndigs = new uint[nl];
                    for (int t = 0; t < nl; t++)
                    {
                        ndigs[t] = this.Digits[t];
                    }
                    this.Digits = ndigs;
                }
                return this;
            }
        }

        /// <summary>
        /// Gets or sets the value of a bit in the integer.
        /// </summary>
        public bool this[int Bit]
        {
            get
            {
                int l = Bit / 32;
                if (l >= this.Digits.Length)
                {
                    return false;
                }
                return (this.Digits[l] & ((uint)1 << (Bit % 32))) > 0;
            }
            set
            {
                int l = Bit / 32;
                if (l >= this.Digits.Length)
                {
                    int nl = l + 1;
                    uint[] ndigs = new uint[nl];
                    for (int t = 0; t < nl; t++)
                    {
                        ndigs[t] = this.Digits[t];
                    }
                    this.Digits = ndigs;
                }
                this.Digits[l] = this.Digits[l] | ((uint)1 << (Bit % 32));
            }
        }

        /// <summary>
        /// Gets the one more than the index of the highest set bit in the integer or 0 if the value is 0.
        /// </summary>
        public int Magnitude
        {
            get
            {
                for (int t = this.Digits.Length - 1; t >= 0; t--)
                {
                    uint d = this.Digits[t];
                    if (d > 0)
                    {
                        int m = 32 * t;
                        while (d > 0)
                        {
                            d = d >> 1;
                            m++;
                        }
                        return m;
                    }
                }
                return 0;
            }
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
        /// Finds the quotient and remainder for the division of A by B.
        /// </summary>
        public static void Divide(BigInt A, BigInt B, out BigInt Quotient, out BigInt Remainder)
        {
            throw new NotImplementedException();
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
        /// Subtracts two integers, giving the result and the carry value. Note that the carry value is to be
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
            return Add(A, B).Reduce;
        }

        public static BigInt operator -(BigInt A, BigInt B)
        {
            return Subtract(A, B).Reduce;
        }

        public static BigInt operator *(BigInt A, BigInt B)
        {
            return Multiply(A, B).Reduce;
        }

        public static BigInt operator /(BigInt A, BigInt B)
        {
            BigInt quo, rem;
            Divide(A, B, out quo, out rem);
            return quo.Reduce;
        }

        public static BigInt operator %(BigInt A, BigInt B)
        {
            BigInt quo, rem;
            Divide(A, B, out quo, out rem);
            return rem.Reduce;
        }

        /// <summary>
        /// The digits, in little-endian order, for the integer.
        /// </summary>
        public uint[] Digits;
    }
}