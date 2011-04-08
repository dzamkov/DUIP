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
            MemoryOutStream mos = new MemoryOutStream();
            Any val = Any.Create(Type.Tuple(new Type[] { Type.Bool, Type.Bool, Type.Reflexive }),
                Tuple.Create<bool, bool, Type>(true, false, Type.Any));
            Type.Any.Serialize(null, val, mos);

            MemoryInStream mis = mos.Read;
            Any nval = Type.Any.Deserialize(null, mis);
        }
    }
}