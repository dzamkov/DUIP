using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Contains symbols and definitions related to boolean values.
    /// </summary>
    public static class Bool
    {
        /// <summary>
        /// The type for boolean values.
        /// </summary>
        /// <remarks>The type of this symbol is "type"</remarks>
        public static readonly Symbol Type;

        /// <summary>
        /// The boolean value of true.
        /// </summary>
        /// <remarks>The type of this symbol is "bool"</remarks>
        public static readonly Symbol True;

        /// <summary>
        /// The boolean value of false.
        /// </summary>
        /// <remarks>The type of this symbol is "bool"</remarks>
        public static readonly Symbol False;

        /// <summary>
        /// A function that determines wether two values of the same type are equal.
        /// </summary>
        /// <remarks>The type of this symbol is "type a => a -> a -> bool</remarks>
        public static readonly Symbol Equal;

        /// <summary>
        /// A function that gets the inverse of a boolean value.
        /// </summary>
        /// <remarks>The type of this symbol is "bool -> bool"</remarks>
        public static readonly Symbol Not;

        /// <summary>
        /// A function that gets wether its two boolean arguments are both true.
        /// </summary>
        /// <remarks>The type of this symbol is "bool -> bool -> bool"</remarks>
        public static readonly Symbol And;

        /// <summary>
        /// A function that gets wether either (or both) of its two boolean arguments are true.
        /// </summary>
        /// <remarks>The type of this symbol is "bool -> bool -> bool"</remarks>
        public static readonly Symbol Or;

        static Bool()
        {
            Expression reflexivetype = Expression.ReflexiveType;

            Type = new Symbol(reflexivetype);
            True = new Symbol(Type);
            False = new Symbol(Type);

            Symbol equaltype = new Symbol(reflexivetype);
            Equal = new Symbol(
                Function.Type(
                    equaltype,
                    Function.Type(equaltype, Function.Type(equaltype, Type))));

            Not = new Symbol(Function.Type(Type, Type));
            And = new Symbol(Function.Type(Type, Function.Type(Type, Type)));
            Or = new Symbol(Function.Type(Type, Function.Type(Type, Type)));
        }
    }

}