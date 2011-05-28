using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A relation between an object of an argument type and an object of a result type defined by a function.
    /// </summary>
    public struct Relation<TArg, TRes>
    {
        /// <summary>
        /// Gets the result of the relation for the given argument.
        /// </summary>
        public TRes this[TArg Argument]
        {
            get
            {
                return this.Evaluate(Argument);
            }
        }

        /// <summary>
        /// Gets the result of the relation for the given argument.
        /// </summary>
        public TRes Evaluate(TArg Argument)
        {
            return this.Function(Argument);
        }

        public static implicit operator Relation<TArg, TRes>(Func<TArg, TRes> Function)
        {
            return new Relation<TArg, TRes>
            {
                Function = Function
            };
        }

        public static implicit operator Relation<TArg, TRes>(TRes Constant)
        {
            return new Relation<TArg, TRes>
            {
                Function = x => Constant
            };
        }

        /// <summary>
        /// The function that can be used to 
        /// </summary>
        public Func<TArg, TRes> Function;
    }
}