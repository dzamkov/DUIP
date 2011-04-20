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

        public BigInt(ulong Value)
        {
            this.Digits = new uint[] { (uint)Value, (uint)(Value >> 32) };
        }

        public BigInt(uint[] Digits)
        {
            this.Digits = Digits;
        }

        /// <summary>
        /// Converts this BigInt into an unsigned 32-bit integer.
        /// </summary>
        public uint ToUInt()
        {
            if (this.Digits.Length > 0)
            {
                return this.Digits[0];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts this BigInt into an unsigned 64-bit integer.
        /// </summary>
        public ulong ToULong()
        {
            if (this.Digits.Length > 0)
            {
                if (this.Digits.Length > 1)
                {
                    return (ulong)this.Digits[0] | ((ulong)this.Digits[1] << 32);
                }
                else
                {
                    return (ulong)this.Digits[0];
                }
            }
            else
            {
                return 0;
            }
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
            while (Offset < this.Size)
            {
                if (cur < Int.Size)
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
                    Offset++;
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
            while (Offset < this.Size)
            {
                if (cur < Int.Size)
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
                    if (carry == 0)
                    {
                        return;
                    }
                    Offset++;
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
            for(int offset = 0; offset < this.Size; offset++)
            {
                uint acarry, bcarry;
                Multiply(this.Digits[offset], Multiplier, out this.Digits[offset], out acarry);
                Add(this.Digits[offset], carry, out this.Digits[offset], out bcarry);
                carry = acarry + bcarry;
            }
        }

        /// <summary>
        /// Copies a BigInt in-place to this value, starting at the given offset in this value's digits. Note that if this integer has a larger
        /// size than the given integer, the higher digits of this integer will remain unchanged.
        /// </summary>
        public void Copy(BigInt Int, int Offset)
        {
            int cur = 0;
            while (Offset < this.Size && cur < Int.Size)
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
        /// Fills this integer with 0's on all digits.
        /// </summary>
        public void ZeroFill()
        {
            for (int t = 0; t < this.Digits.Length; t++)
            {
                this.Digits[t] = 0;
            }
        }

        /// <summary>
        /// Trims excess zero digits from the integer.
        /// </summary>
        public BigInt Reduce
        {
            get
            {
                int nl = 0;
                for (int t = this.Size - 1; t >= 0; t--)
                {
                    if (this.Digits[t] > 0)
                    {
                        nl = t + 1;
                        break;
                    }
                }
                if (nl < this.Size)
                {
                    BigInt x = this;
                    x.Resize(nl);
                    return x;
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
                if (l >= this.Size)
                {
                    return false;
                }
                return (this.Digits[l] & ((uint)1 << (Bit % 32))) > 0;
            }
            set
            {
                int l = Bit / 32;
                if (l >= this.Size)
                {
                    int nl = l + 1;
                    this.Resize(nl);
                }
                if (value)
                {
                    this.Digits[l] = this.Digits[l] | ((uint)1 << (Bit % 32));
                }
                else
                {
                    this.Digits[l] = this.Digits[l] ^ ((uint)1 << (Bit % 32));
                }
            }
        }

        /// <summary>
        /// Gets the one more than the index of the highest set bit in the integer or 0 if the value is 0.
        /// </summary>
        public int Magnitude
        {
            get
            {
                for (int t = this.Size - 1; t >= 0; t--)
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

        public override string ToString()
        {
            string x = "";

            // This is probably very inefficent
            BigInt t = this;
            while (t > 0)
            {
                BigInt q, r; BigInt.Divide(t, 10, out q, out r);
                r.Resize(1);
                t = q;
                x = r.Digits[0].ToString() + x;
            }

            if (x.Length == 0)
            {
                return "0";
            }

            return x;
        }

        public override bool Equals(object obj)
        {
            BigInt? bi = obj as BigInt?;
            if (bi != null)
            {
                return bi.Value == this;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Left-shifts the integer in-place by the specified amount. This increases the magnitude by the amount.
        /// </summary>
        public void Shift(int Amount)
        {
            if (Amount > 0)
            {
                int l = Amount / 32;
                Amount %= 32;
                for (int t = this.Size - 1; t >= 0; t--)
                {
                    int ai = t - l;
                    int bi = t - l - 1;
                    uint a = ai >= 0 ? (this.Digits[ai] << Amount) : 0;
                    uint b = bi >= 0 ? (this.Digits[bi] >> (32 - Amount)) : 0;
                    this.Digits[t] = a | b;
                }
            }
            if (Amount < 0)
            {
                Amount = -Amount;
                int l = Amount / 32;
                Amount %= 32;
                int m = this.Digits.Length;
                for (int t = 0; t < this.Size; t++)
                {
                    int ai = t + l;
                    int bi = t + l + 1;
                    uint a = ai < m ? (this.Digits[ai] >> Amount) : 0;
                    uint b = bi < m ? (this.Digits[bi] << (32 - Amount)) : 0;
                    this.Digits[t] = a | b;
                }
            }
        }

        /// <summary>
        /// Resizes the integer to have the specified amount of digits. Newly created digits are zero-filled.
        /// </summary>
        public void Resize(int Digits)
        {
            uint[] ndigs = new uint[Digits];
            for (int t = 0; t < Math.Min(Digits, this.Size); t++)
            {
                ndigs[t] = this.Digits[t];
            }
            this.Digits = ndigs;
        }

        /// <summary>
        /// Gets the amount of digits this integer has.
        /// </summary>
        public int Size
        {
            get
            {
                return this.Digits.Length;
            }
        }

        /// <summary>
        /// Adds two integers together.
        /// </summary>
        public static BigInt Add(BigInt A, BigInt B)
        {
            BigInt res = BigInt.Empty(Math.Max(A.Size, B.Size) + 1);
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
            BigInt res = BigInt.Empty(Math.Max(A.Size, B.Size) + 1);
            res.Copy(A);
            res.Subtract(B);
            return res;
        }

        /// <summary>
        /// Multiplies two integers together.
        /// </summary>
        public static BigInt Multiply(BigInt A, BigInt B)
        {
            BigInt res = BigInt.Empty(A.Size + B.Size);
            BigInt temp = BigInt.Empty(A.Size + 1);
            for (int t = 0; t < B.Size; t++)
            {
                temp.ZeroFill();
                temp.Copy(A);
                temp.Multiply(B.Digits[t]);
                res.Add(temp, t);
            }
            return res;
        }

        /// <summary>
        /// Performs (Base ^ Exponent) % Modulus in an efficent way.
        /// </summary>
        public static BigInt PowMod(BigInt Base, BigInt Exponent, BigInt Modulus)
        {
            BigInt tempexp = BigInt.Empty(Exponent.Size);
            tempexp.Copy(Exponent);
            return _PowMod(Base, tempexp, Modulus);
        }

        private static BigInt _PowMod(BigInt Base, BigInt TempExponent, BigInt Modulus)
        {
            if (TempExponent == 0)
            {
                return 1;
            }
            if (TempExponent == 1)
            {
                return Base % Modulus;
            }
            if (TempExponent[0])
            {
                TempExponent[0] = false;
                return Multiply(_PowMod(Base, TempExponent, Modulus), Base) % Modulus;
            }
            else
            {
                TempExponent.Shift(-1);
                BigInt hp = _PowMod(Base, TempExponent, Modulus);
                return Multiply(hp, hp) % Modulus;
            }
        }

        /// <summary>
        /// Gets wether A has a lesser value than B.
        /// </summary>
        public static bool Lesser(BigInt A, BigInt B)
        {
            int c = Math.Max(A.Digits.Length, B.Digits.Length);
            while (c >= 0)
            {
                uint a = c < A.Digits.Length ? A.Digits[c] : 0;
                uint b = c < B.Digits.Length ? B.Digits[c] : 0;
                if (a < b)
                {
                    return true;
                }
                if (a > b)
                {
                    return false;
                }
                c--;
            }
            return false;
        }

        /// <summary>
        /// Gets wether A has a value equal to B's.
        /// </summary>
        public static bool Equal(BigInt A, BigInt B)
        {
            int c = Math.Max(A.Digits.Length, B.Digits.Length);
            while (c >= 0)
            {
                uint a = c < A.Digits.Length ? A.Digits[c] : 0;
                uint b = c < B.Digits.Length ? B.Digits[c] : 0;
                if (a != b)
                {
                    return false;
                }
                c--;
            }
            return true;
        }

        /// <summary>
        /// Finds the quotient and remainder for the division of A by B.
        /// </summary>
        public static void Divide(BigInt A, BigInt B, out BigInt Quotient, out BigInt Remainder)
        {
            if (B > A)
            {
                Remainder = A;
                Quotient = Zero;
            }
            Remainder = BigInt.Empty(A.Size);
            Remainder.Copy(A);
            BigInt btemp = BigInt.Empty(A.Size);
            btemp.Copy(B);
            int bitoff = A.Magnitude - B.Magnitude;
            btemp.Shift(bitoff);
            Quotient = BigInt.Empty(bitoff/ 32 + 1);
            
            while (bitoff >= 0)
            {
                if (btemp <= Remainder)
                {
                    Remainder.Subtract(btemp);
                    Quotient[bitoff] = true;
                }
                btemp.Shift(-1);
                bitoff--;
            }
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

        public static implicit operator BigInt(ulong Int)
        {
            return new BigInt(Int);
        }

        public static explicit operator uint(BigInt Int)
        {
            return Int.ToUInt();
        }

        public static explicit operator ulong(BigInt Int)
        {
            return Int.ToULong();
        }

        public static explicit operator int(BigInt Int)
        {
            return (int)Int.ToUInt();
        }

        public static explicit operator long(BigInt Int)
        {
            return (long)Int.ToULong();
        }

        public static BigInt operator +(BigInt A, BigInt B)
        {
            return Add(A, B).Reduce;
        }

        public static BigInt operator +(BigInt A, uint B)
        {
            return A + (BigInt)B;
        }

        public static BigInt operator ++(BigInt A)
        {
            for (int t = 0; t < A.Digits.Length; t++)
            {
                uint cur = A.Digits[t];
                cur++;
                A.Digits[t] = cur;
                if (cur > 0)
                {
                    break;
                }
            }
            return A;
        }

        public static BigInt operator --(BigInt A)
        {
            for (int t = 0; t < A.Digits.Length; t++)
            {
                uint cur = A.Digits[t];
                cur--;
                A.Digits[t] = cur;
                if (cur < uint.MaxValue)
                {
                    break;
                }
            }
            return A;
        }

        public static BigInt operator -(BigInt A, BigInt B)
        {
            return Subtract(A, B).Reduce;
        }

        public static BigInt operator -(BigInt A, uint B)
        {
            return A - (BigInt)B;
        }

        public static BigInt operator *(BigInt A, BigInt B)
        {
            return Multiply(A, B).Reduce;
        }

        public static BigInt operator *(BigInt A, uint B)
        {
            BigInt c = BigInt.Empty(A.Size + 1);
            c.Copy(A);
            c.Multiply(B);
            return c.Reduce;
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

        public static bool operator <(BigInt A, BigInt B)
        {
            return Lesser(A, B);
        }

        public static bool operator >(BigInt A, BigInt B)
        {
            return Lesser(B, A);
        }

        public static bool operator <=(BigInt A, BigInt B)
        {
            return !Lesser(B, A);
        }

        public static bool operator >=(BigInt A, BigInt B)
        {
            return !Lesser(A, B);
        }

        public static bool operator ==(BigInt A, BigInt B)
        {
            return Equal(A, B);
        }

        public static bool operator !=(BigInt A, BigInt B)
        {
            return !Equal(A, B);
        }

        /// <summary>
        /// The digits, in little-endian order, for the integer.
        /// </summary>
        public uint[] Digits;
    }
}