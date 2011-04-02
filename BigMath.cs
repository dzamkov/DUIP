using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Contains functions for performing mathematical operations on variable sized byte arrays. The byte arrays
    /// are assumed to be positive little endian numbers.
    /// </summary>
    public static class BigMath
    {
        /// <summary>
        /// Adds the value of In to Out.
        /// </summary>
        public static void Add(byte[] In, ref byte[] Out)
        {
            byte carry = 0;
            for (int t = 0; t < In.Length; t++)
            {
                if (t < Out.Length)
                {
                    byte ncarry;
                    Add(carry, Out[t], out Out[t], out carry);
                    Add(In[t], Out[t], out Out[t], out ncarry);
                    carry += ncarry;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Subtracts the value of In from Out.
        /// </summary>
        public static void Subtract(byte[] In, ref byte[] Out)
        {
            byte carry = 0;
            for (int t = 0; t < In.Length; t++)
            {
                if (t < Out.Length)
                {
                    byte ncarry;
                    Subtract(Out[t], carry, out Out[t], out carry);
                    Subtract(Out[t], In[t], out Out[t], out ncarry);
                    carry += ncarry;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Multiplies two values together.
        /// </summary>
        public static void Multiply(byte[] A, byte[] B, ref byte[] Out)
        {
            for (int x = 0; x < A.Length; x++)
            {
                for (int y = 0; y < B.Length; y++)
                {
                    int i = x + y;
                    if (i < Out.Length)
                    {
                        byte a = A[x];
                        byte b = B[y];
                        byte add;
                        byte carry;
                        Multiply(a, b, out add, out carry);
                        Add(add, Out[i], out Out[i], out add);
                        carry += add;
                        while (carry > 0 && (++i) < Out.Length)
                        {
                            Add(carry, Out[i], out Out[i], out carry);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the quotient and remainder when dividing A by B.
        /// </summary>
        public static void Divide(ref byte[] A, byte[] B, ref byte[] Quo, ref byte[] Rem)
        {

        }

        /// <summary>
        /// Decrement a value by one.
        /// </summary>
        public static void Decrement(ref byte[] A)
        {
            for (int x = 0; x < A.Length; x++)
            {
                byte val = A[x];
                if (val > 0)
                {
                    A[x] = --val;
                    break;
                }
                else
                {
                    A[x] = 255;
                }
            }
        }

        /// <summary>
        /// Adds two bytes together.
        /// </summary>
        public static void Add(byte A, byte B, out byte Out, out byte Carry)
        {
            ushort a = (ushort)A; ushort b = (ushort)B;
            a += b;
            Out = (byte)(a);
            Carry = (byte)(a / 256);
        }

        /// <summary>
        /// Subtracts two bytes. Note that the carry byte is negative.
        /// </summary>
        public static void Subtract(byte A, byte B, out byte Out, out byte Carry)
        {
            short a = (short)A; short b = (short)B;
            a -= b;
            Out = (byte)(a);
            Carry = (byte)((255 - a) / 256);
        }

        /// <summary>
        /// Multiplies two bytes together.
        /// </summary>
        public static void Multiply(byte A, byte B, out byte Out, out byte Carry)
        {
            ushort a = (ushort)A; ushort b = (ushort)B;
            a *= b;
            Out = (byte)(a);
            Carry = (byte)(a / 256);
        }
    }
}