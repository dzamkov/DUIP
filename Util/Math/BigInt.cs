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
        /// The digits, in little-endian order, for the integer.
        /// </summary>
        public uint[] Digits;
    }
}