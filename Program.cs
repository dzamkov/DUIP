using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Contains functions and information related to the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program release version number.
        /// </summary>
        public const int Version = 1;

        /// <summary>
        /// Program main entry point.
        /// </summary>
        public static void Main(string[] Args)
        {
            byte[] testdata = new byte[] { 5, 6, 7, 8 };
            Cipher cipher = new RSACipher(4);
            Key d, e;
            cipher.GenerateKeys(new Random(), out e, out d);
        }
    }
}