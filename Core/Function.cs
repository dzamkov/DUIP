using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A pure transformation of values.
    /// </summary>
    public class Function<TArg, TRes>
    {
        internal Function()
        {

        }
    }

    /// <summary>
    /// The type for functions of a certain argument and result type.
    /// </summary>
    public class FunctionType<TArg, TRes> : Type<Function<TArg, TRes>>
    {
        internal FunctionType(Type<TArg> Argument, Type<TRes> Result)
        {
            this._Argument = Argument;
            this._Result = Result;
        }

        /// <summary>
        /// The argument (input) type of the function.
        /// </summary>
        public Type<TArg> Argument
        {
            get
            {
                return this._Argument;
            }
        }

        /// <summary>
        /// The result (output) type of the function.
        /// </summary>
        public Type<TRes> Result
        {
            get
            {
                return this._Result;
            }
        }

        private Type<Arg> _Argument;
        private Type<TRes> _Result;
    }

    /// <summary>
    /// Helper class for function-related tasks.
    /// </summary>
    public static class Function
    {
        
    }
}