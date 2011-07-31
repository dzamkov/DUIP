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

            Type = new Symbol(reflexivetype, "bool");
            True = new Symbol(Type, "true");
            False = new Symbol(Type, "false");

            Symbol equaltype = new Symbol(reflexivetype, "equal");
            Equal = new Symbol(
                Function.Type(
                    equaltype,
                    Function.Type(equaltype, Function.Type(equaltype, Type))));

            Not = new Symbol(Function.Type(Type, Type), "not");
            And = new Symbol(Function.Type(Type, Function.Type(Type, Type)), "and");
            Or = new Symbol(Function.Type(Type, Function.Type(Type, Type)), "or");

            Literal<bool>.InstanceType = Type;
            Literal<bool>.Equal = (x, y) => x == y;
        }

        /// <summary>
        /// Defines relations involving bools in the given scope.
        /// </summary>
        public static void Define(Scope Scope)
        {
            Scope.DefineProperty(0, Not + False, True);
            Scope.DefineProperty(0, Not + True, False);
            Scope.DefineProperty(1, Not + Not + Term.Get(0), Term.Get(0));

            Scope.DefineCommutativeProperty(0, And);
            Scope.DefineProperty(0, And + True + True, True);
            Scope.DefineProperty(1, And + False + Term.Get(0), False);

            Scope.DefineCommutativeProperty(0, Or);
            Scope.DefineProperty(0, Or + False + False, False);
            Scope.DefineProperty(1, Or + True + Term.Get(0), True);

            Scope.DefineCommutativeProperty(1, Equal + Term.Get(0));
            Scope.DefineProperty(2, Equal + Term.Get(0) + Term.Get(1) + Term.Get(1), True);
            Scope.DefineProperty(0, Equal + Type + True + False, False);

            Scope.DefineProperty(0, True, new Literal<bool>(true));
            Scope.DefineProperty(0, False, new Literal<bool>(false));
        }
    }

}