using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A pure transformation of values.
    /// </summary>
    public abstract class Function<TArg, TRes>
    {
        internal Function()
        {

        }

        /// <summary>
        /// Calls the function with the given argument.
        /// </summary>
        public abstract Query<TRes> Call(TArg Arg);
    }

    /// <summary>
    /// A function that returns the same constant value for any parameter.
    /// </summary>
    public class ConstantFunction<TArg, TRes> : Function<TArg, TRes>
    {
        public ConstantFunction(TRes Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// Gets the value of the constant function.
        /// </summary>
        public TRes Value
        {
            get
            {
                return this._Value;
            }
        }

        public override Query<TRes> Call(TArg Arg)
        {
            return this._Value;
        }

        private TRes _Value;
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

        private Type<TArg> _Argument;
        private Type<TRes> _Result;
    }

    /// <summary>
    /// Helper class for function-related tasks.
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// Creates a function type given the argument and result type.
        /// </summary>
        public static FunctionType<TArg, TRes> Type<TArg, TRes>(Type<TArg> Argument, Type<TRes> Result)
        {
            return new FunctionType<TArg, TRes>(Argument, Result);
        }

        /// <summary>
        /// Creates a function with a constant value.
        /// </summary>
        public static ConstantFunction<TArg, TRes> Constant<TArg, TRes>(TRes Value)
        {
            return new ConstantFunction<TArg, TRes>(Value);
        }
    }
}