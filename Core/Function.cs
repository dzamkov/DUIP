using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Method of storing a function.
    /// </summary>
    public enum FunctionMode : byte
    {
        Constant
    }

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

        /// <summary>
        /// Serializes this function to a stream.
        /// </summary>
        public abstract void Serialize(FunctionType<TArg, TRes> Type, OutByteStream Stream);

        /// <summary>
        /// Gets the mode to be used for storing this function.
        /// </summary>
        public abstract FunctionMode Mode { get; }
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

        public override void Serialize(FunctionType<TArg, TRes> Type, OutByteStream Stream)
        {
            Type.Result.Serialize(this._Value, Stream);
        }

        public override FunctionMode Mode
        {
            get
            {
                return FunctionMode.Constant;
            }
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

        public override void Serialize(Function<TArg, TRes> Instance, OutByteStream Stream)
        {
            FunctionMode mode = Instance.Mode;
            Stream.Write((byte)mode);
            Instance.Serialize(this, Stream);
        }

        public override Query<Function<TArg, TRes>> Deserialize(InByteStream Stream)
        {
            FunctionMode mode = (FunctionMode)Stream.Read();
            switch (mode)
            {
                case FunctionMode.Constant:
                    return this._Result.Deserialize(Stream).Bind<Function<TArg, TRes>>(delegate(TRes Value)
                    {
                        return new ConstantFunction<TArg, TRes>(Value);
                    });
            }
            return null;
        }

        private Type<TArg> _Argument;
        private Type<TRes> _Result;
    }

    /// <summary>
    /// Helper class for function-related tasks.
    /// </summary>
    public static class Function
    {
        
    }
}