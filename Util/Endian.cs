using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Contains functions related to the manipulation of the endianess of numerical values.
    /// </summary>
    public static class Endian
    {
        /// <summary>
        /// Swaps the endian of a 64-bit integer.
        /// </summary>
        public static void Swap(ref long Value)
        {
            ulong uval = (ulong)Value;
            Value = (long)(
               (0x00000000000000FF) & (uval >> 56) | 
               (0x000000000000FF00) & (uval >> 40) | 
               (0x0000000000FF0000) & (uval >> 24) | 
               (0x00000000FF000000) & (uval >> 8) | 
               (0x000000FF00000000) & (uval << 8) | 
               (0x0000FF0000000000) & (uval << 24) | 
               (0x00FF000000000000) & (uval << 40) | 
               (0xFF00000000000000) & (uval << 56));
        }

        /// <summary>
        /// Swaps the endian of a 32-bit integer.
        /// </summary>
        public static void Swap(ref int Value)
        {
            uint uval = (uint)Value;
            Value = (int)(
                (0x000000FF) & (uval >> 24) |
                (0x0000FF00) & (uval >> 8) |
                (0x00FF0000) & (uval << 8) |
                (0xFF000000) & (uval << 24));
        }
    }
}