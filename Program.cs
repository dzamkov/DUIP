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
            MemoryOutStream os = new MemoryOutStream();

            Type bigtype = Type.Function(Type.Function(Type.Reflexive, Type.Reflexive), Type.Reflexive);

            Type.Reflexive.Serialize(null, bigtype, os);

            Type nbigtype = Type.Reflexive.Deserialize(null, os.Read);
        }
    }
}