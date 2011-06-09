using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Defines a relation between an argument and a result.
    /// </summary>
    public abstract class Function
    {
        /// <summary>
        /// Tries evaluating this function with the given argument. Returns false if there was a computional
        /// error (infinite loop, excessive memory usage, took too long, etc).
        /// </summary>
        public abstract bool Evaluate(object Argument, out object Result);

        /// <summary>
        /// Gets the identity function.
        /// </summary>
        public static IdentityFunction Identity
        {
            get
            {
                return IdentityFunction.Singleton;
            }
        }

        /// <summary>
        /// Gets a function that always returns the given constant value.
        /// </summary>
        public static ConstantFunction Constant(object Value)
        {
            return new ConstantFunction(Value);
        }

        /// <summary>
        /// Gets a call function.
        /// </summary>
        public static CallFunction Call(Type Inner, Function Function, Function Argument)
        {
            return new CallFunction(Inner, Function, Argument);
        }
    }

    /// <summary>
    /// A function which returns its argument.
    /// </summary>
    public sealed class IdentityFunction : Function
    {
        private IdentityFunction()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IdentityFunction Singleton = new IdentityFunction();

        public override bool Evaluate(object Argument, out object Result)
        {
            Result = Argument;
            return true;
        }
    }

    /// <summary>
    /// A function whose result is always a certain value.
    /// </summary>
    public sealed class ConstantFunction : Function
    {
        public ConstantFunction(object Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// Gets the constant value of this function.
        /// </summary>
        public object Value
        {
            get
            {
                return this._Value;
            }
        }

        public override bool Evaluate(object Argument, out object Result)
        {
            Result = this._Value;
            return true;
        }

        private object _Value;
    }

    /// <summary>
    /// A function that derives its result by calling a function with an argument, with both parts
    /// being defined by functions.
    /// </summary>
    /// <remarks>This type of function can be thought of as the composition of two constant functions of types
    /// (A -> B -> C) and (A -> B) to form a function of type (A -> C) with B being the inner type.</remarks>
    public sealed class CallFunction : Function
    {
        public CallFunction(Type InnerType, Function Function, Function Argument)
        {
            this._InnerType = InnerType;
            this._Function = Function;
            this._Argument = Argument;
        }

        /// <summary>
        /// Gets the inner type (the type of the argument) for this function call.
        /// </summary>
        public Type InnerType
        {
            get
            {
                return this._InnerType;
            }
        }

        /// <summary>
        /// Get a function that, when evaluated with the argument of this function, will return the function
        /// component of the call.
        /// </summary>
        public Function Function
        {
            get
            {
                return this._Function;
            }
        }

        /// <summary>
        /// Get a function that, when evaluated with the argument of this function, will return the argument
        /// component of the call.
        /// </summary>
        public Function Argument
        {
            get
            {
                return this._Argument;
            }
        }

        public override bool Evaluate(object Argument, out object Result)
        {
            object func;
            object arg;
            if (this._Function.Evaluate(Argument, out func))
            {
                if (this._Argument.Evaluate(Argument, out arg))
                {
                    (func as Function).Evaluate(arg, out Result);
                    return true;
                }
            }
            Result = null;
            return false;
        }

        private Type _InnerType;
        private Function _Function;
        private Function _Argument;
    }

    

    /// <summary>
    /// A type for a relation (or rather, a specific definition of a relation) between an argument and a result of certain types.
    /// </summary>
    public class FunctionType : Type
    {
        private FunctionType()
        {

        }

        /// <summary>
        /// Gets a function type for the given argument and result types.
        /// </summary>
        public static FunctionType Get(Type Argument, Type Result)
        {
            foreach (FunctionType type in _Types)
            {
                if (Type.Equal(type._Argument, Argument) && Type.Equal(type._Result, Result))
                {
                    return type;
                }
            }
            FunctionType ntype = new FunctionType
            {
                _Argument = Argument,
                _Result = Result
            };
            _Types.Register(ntype);
            return ntype;
        }

        /// <summary>
        /// A registry of available function types.
        /// </summary>
        private static Registry<FunctionType> _Types = new Registry<FunctionType>();

        public override bool Equal(object A, object B)
        {
            return A == B;
        }

        public override ISerialization<object> GetSerialization(Context Context)
        {
            return new FunctionSerialization
            {
                Context = Context,
                Type = this,
                TypeSerialization = Reflexive.GetSerialization(Context),
                ResultSerialization = this._Result.GetSerialization(Context)
            };
        }

        /// <summary>
        /// Gets the type required for an argument to a function of this type.
        /// </summary>
        public Type Argument
        {
            get
            {
                return this._Argument;
            }
        }

        /// <summary>
        /// Gets the type of a result from a function of this type.
        /// </summary>
        public Type Result
        {
            get
            {
                return this._Result;
            }
        }

        private Type _Argument;
        private Type _Result;
    }

    /// <summary>
    /// A serialization method for functions.
    /// </summary>
    public class FunctionSerialization : ISerialization<Function>, ISerialization<object>
    {
        /// <summary>
        /// Specifies a possible method to use for serialization.
        /// </summary>
        public enum Method
        {
            Identity,
            Constant,
            Call
        }

        public void Serialize(Function Object, OutStream Stream)
        {
            if (Object == IdentityFunction.Singleton)
            {
                Stream.WriteByte((byte)Method.Identity);
                return;
            }

            ConstantFunction cf = Object as ConstantFunction;
            if (cf != null)
            {
                Stream.WriteByte((byte)Method.Constant);
                this.ResultSerialization.Serialize(cf.Value, Stream);
                return;
            }

            CallFunction ccf = Object as CallFunction;
            if (ccf != null)
            {
                Stream.WriteByte((byte)Method.Call);
                this.TypeSerialization.Serialize(ccf.InnerType, Stream);

                FunctionType functiontype = FunctionType.Get(this.Type.Argument, FunctionType.Get(ccf.InnerType, this.Type.Result));
                FunctionType argumenttype = FunctionType.Get(this.Type.Argument, ccf.InnerType);

                functiontype.GetSerialization(this.Context).Serialize(ccf.Function, Stream);
                argumenttype.GetSerialization(this.Context).Serialize(ccf.Argument, Stream);
                return;
            }
        }

        public Function Deserialize(InStream Stream)
        {
            switch ((Method)Stream.ReadByte())
            {
                case Method.Identity:
                    if (!DUIP.Type.Equal(this.Type.Argument, this.Type.Result))
                    {
                        throw new DeserializationException();
                    }
                    return IdentityFunction.Singleton;
                case Method.Constant:
                    object value = this.ResultSerialization.Deserialize(Stream);
                    return new ConstantFunction(value);
                case Method.Call:
                    Type inner = this.TypeSerialization.Deserialize(Stream) as Type;

                    FunctionType functiontype = FunctionType.Get(this.Type.Argument, FunctionType.Get(inner, this.Type.Result));
                    FunctionType argumenttype = FunctionType.Get(this.Type.Argument, inner);

                    object function = functiontype.GetSerialization(this.Context).Deserialize(Stream);
                    object argument = argumenttype.GetSerialization(this.Context).Deserialize(Stream);
                    return new CallFunction(inner, function as Function, argument as Function);
                default:
                    throw new DeserializationException();
            }
        }

        void ISerialization<object>.Serialize(object Object, OutStream Stream)
        {
            this.Serialize(Object as Function, Stream);
        }

        object ISerialization<object>.Deserialize(InStream Stream)
        {
            return this.Deserialize(Stream);
        }

        public Maybe<long> Size
        {
            get
            {
                return Maybe<long>.Nothing;
            }
        }

        public Context Context;
        public FunctionType Type;
        public ISerialization<object> ResultSerialization;
        public ISerialization<object> TypeSerialization;
    }
}