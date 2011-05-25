using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A relation between an argument and a result.
    /// </summary>
    public class Function<TArg, TRes>
    {

    }

    /// <summary>
    /// A type for a relation (or rather, a specific definition of a relation) between an argument and a result of certain types.
    /// </summary>
    public class FunctionType<TArg, TRes> : Type<Function<TArg, TRes>>
    {
        public override bool Equal(Function<TArg, TRes> A, Function<TArg, TRes> B)
        {
            return A == B;
        }

        /// <summary>
        /// Gets the type required for an argument to a function of this type.
        /// </summary>
        public Type<TArg> Argument
        {
            get
            {
                return this._Argument;
            }
        }

        /// <summary>
        /// Gets the type of a result from a function of this type.
        /// </summary>
        public Type<TRes> Result
        {
            get
            {
                return this._Result;
            }
        }

        /// <summary>
        /// Evaluates a given function (of this type) for a given argument.
        /// </summary>
        public TRes Evaluate(Function<TArg, TRes> Function, TArg Argument)
        {
            throw new NotImplementedException();
        }

        private Type<TArg> _Argument;
        private Type<TRes> _Result;
    }   
}