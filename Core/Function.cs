using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A relation between an argument and a result.
    /// </summary>
    public abstract class Function
    {
        /// <summary>
        /// Tries evaluating this function with the given argument. A computational exception may be thrown
        /// in the case of a computational error.
        /// </summary>
        public abstract object Evaluate(object Argument);

        /// <summary>
        /// Gets the identity function.
        /// </summary>
        public static IdentityFunction Identity
        {
            get
            {
                return IdentityFunction.Instance;
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
    /// An exception thrown when a computational error (infinite loop, excessive memory usage, took too long, 
    /// undecidable problem, etc) occurs while evaluating a function.
    /// </summary>
    public class ComputationalException : Exception
    {

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
        public static readonly IdentityFunction Instance = new IdentityFunction();

        public override object Evaluate(object Argument)
        {
            return Argument;
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

        public override object Evaluate(object Argument)
        {
            return this._Value;
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

        public override object Evaluate(object Argument)
        {
            object func = this._Function.Evaluate(Argument);
            object arg = this._Argument.Evaluate(Argument);
            return ((Function)func).Evaluate(arg);
        }

        private Type _InnerType;
        private Function _Function;
        private Function _Argument;
    }

    /// <summary>
    /// A type for functions of a certain argument and return type (a relation between types).
    /// </summary>
    /// <remarks>Two functions are considered equivalent if and only if their results are equivalent when evaluated
    /// with all instances of the argument type.</remarks>
    [Kind(1)]
    public class FunctionType : Type
    {
        public FunctionType(Type Argument, Type Result)
        {
            this._Argument = Argument;
            this._Result = Result;
        }

        public override bool Equal(object A, object B)
        {
            if (A == B)
            {
                return true;
            }

            // This is an undecidable problem anyway
            throw new ComputationalException();
        }

        /// <summary>
        /// Determines wether two function types are equal.
        /// </summary>
        public static bool TypeEquals(FunctionType A, FunctionType B)
        {
            return
                Type.Equal(A._Argument, B._Argument) &&
                Type.Equal(A._Result, B._Result);
        }

        public static bool TypeEquals(Type A, Type B)
        {
            return TypeEquals((FunctionType)A, (FunctionType)B);
        }

        public static ISerialization<Type> GetTypeSerialization(Context Context)
        {
            return new _FunctionTypeSerialization()
            {
                TypeSerialization = new TypeSerialization(Context)
            };
        }

        private sealed class _FunctionTypeSerialization : ISerialization<FunctionType>, ISerialization<Type>
        {
            public void Serialize(FunctionType Object, OutStream Stream)
            {
                ISerialization<Type> typeserialization = this.TypeSerialization;
                typeserialization.Serialize(Object._Argument, Stream);
                typeserialization.Serialize(Object._Result, Stream);
            }

            public FunctionType Deserialize(InStream Stream)
            {
                ISerialization<Type> typeserialization = this.TypeSerialization;
                return new FunctionType(
                    typeserialization.Deserialize(Stream),
                    typeserialization.Deserialize(Stream));
            }

            void ISerialization<Type>.Serialize(Type Object, OutStream Stream)
            {
                this.Serialize((FunctionType)Object, Stream);
            }

            Type ISerialization<Type>.Deserialize(InStream Stream)
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

            public ISerialization<Type> TypeSerialization;
        }

        public override ISerialization<object> GetSerialization(Context Context)
        {
            return new FunctionSerialization
            {
                Context = Context,
                Type = this,
                TypeSerialization = ReflexiveType.Instance.GetSerialization(Context),
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
            if (Object == IdentityFunction.Instance)
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

                FunctionType functiontype = new FunctionType(this.Type.Argument, new FunctionType(ccf.InnerType, this.Type.Result));
                FunctionType argumenttype = new FunctionType(this.Type.Argument, ccf.InnerType);

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
                    return IdentityFunction.Instance;
                case Method.Constant:
                    object value = this.ResultSerialization.Deserialize(Stream);
                    return new ConstantFunction(value);
                case Method.Call:
                    Type inner = this.TypeSerialization.Deserialize(Stream) as Type;

                    FunctionType functiontype = new FunctionType(this.Type.Argument, new FunctionType(inner, this.Type.Result));
                    FunctionType argumenttype = new FunctionType(this.Type.Argument, inner);

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