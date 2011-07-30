using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Contains symbols and definitions related to types.
    /// </summary>
    public static class Type
    {
        /// <summary>
        /// A symbol representing the reflexive type, a type of all types, including itself.
        /// </summary>
        /// <remarks>The type of this symbol is "type"</remarks>
        public static readonly Symbol Reflexive;

        static Type()
        {
            Reflexive = new Symbol();
            Reflexive.SetType(Reflexive);
        }
    }
}