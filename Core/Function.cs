using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Identifies a method of describing a function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FunctionKind : Attribute
    {
        public FunctionKind(byte ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// The identifier for this function kind.
        /// </summary>
        public byte ID;

        static FunctionKind()
        {
            _ForID = new Dictionary<byte, List<FunctionKind>>();
            _ForType = new Dictionary<System.Type, FunctionKind>();
            foreach (var kvp in Reflection.SearchAttributes<FunctionKind>())
            {
                FunctionKind fk = kvp.Value;

                _ForType[kvp.Key] = fk;
                List<FunctionKind> forid;
                if (!_ForID.TryGetValue(fk.ID, out forid))
                {
                    _ForID[fk.ID] = forid = new List<FunctionKind>();
                }
                forid.Add(fk);

                Maybe<Func<FunctionType, bool>> isvalid = Reflection.Cast<Func<FunctionType, bool>>(kvp.Key, "IsValid");
                if (isvalid.HasValue)
                {
                    fk._IsValid = isvalid.Value;
                }
                else
                {
                    fk._IsValid = (x) => true;
                }

                Maybe<Function> instance = Reflection.Cast<Function>(kvp.Key, "Instance");
                if (instance.HasValue)
                {
                    ConstantSerialization<Function> serialization = new ConstantSerialization<Function>(instance.Value);
                    fk._GetSerialization = (x) => serialization;
                    continue;
                }

                fk._GetSerialization = Reflection.Cast<Func<FunctionType, ISerialization<Function>>>(kvp.Key, "GetSerialization").Value;
            }
        }

        public static FunctionKind ForID(byte ID, FunctionType Type)
        {
            foreach (FunctionKind fk in _ForID[ID])
            {
                if (fk._IsValid(Type))
                {
                    return fk;
                }
            }
            return null;
        }

        public static FunctionKind ForType(System.Type Type)
        {
            return _ForType[Type];
        }

        public ISerialization<Function> GetSerialization(FunctionType Type)
        {
            return this._GetSerialization(Type);
        }

        private static Dictionary<byte, List<FunctionKind>> _ForID;
        private static Dictionary<System.Type, FunctionKind> _ForType;
        private Func<FunctionType, ISerialization<Function>> _GetSerialization;
        private Func<FunctionType, bool> _IsValid;
    }

    /// <summary>
    /// A pure function that relates values of certain types.
    /// </summary>
    public abstract class Function
    {
        /// <summary>
        /// Tries evaluating this function with the given argument. A computational exception may be thrown
        /// in the case of a computational error.
        /// </summary>
        public abstract object Evaluate(object Argument);
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
    [FunctionKind(0)]
    public sealed class IdentityFunction : Function
    {
        private IdentityFunction()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IdentityFunction Instance = new IdentityFunction();

        public static bool IsValid(FunctionType Type)
        {
            return ReflexiveType.Equal(Type.Argument, Type.Result);
        }

        public override object Evaluate(object Argument)
        {
            return Argument;
        }
    }

    /// <summary>
    /// A function whose result is always a certain value.
    /// </summary>
    [FunctionKind(1)]
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

        public static ISerialization<Function> GetSerialization(FunctionType Type)
        {
            return new _Serialization
            {
                ValueSerialization = Type.Result.Serialization
            };
        }

        private class _Serialization : ISerialization<Function>
        {
            public void Write(Function Object, OutStream Stream)
            {
                this.ValueSerialization.Write(((ConstantFunction)Object)._Value, Stream);
            }

            Function ISerialization<Function>.Read(InStream Stream)
            {
                return new ConstantFunction(this.ValueSerialization.Read(Stream));
            }

            Maybe<long> ISerialization<Function>.Size
            {
                get 
                {
                    return this.ValueSerialization.Size;
                }
            }

            public ISerialization<object> ValueSerialization;
        }

        private object _Value;
    }

    /// <summary>
    /// A function that derives its result by calling a function with an argument, with both parts
    /// being defined by functions.
    /// </summary>
    /// <remarks>This type of function can be thought of as the composition of two constant functions of types
    /// (A -> B -> C) and (A -> B) to form a function of type (A -> C) with B being the inner type.</remarks>
    [FunctionKind(2)]
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

        /// <summary>
        /// Gets the type of "Function" with the given source function type and inner type.
        /// </summary>
        public static FunctionType GetFunctionType(FunctionType SourceType, Type InnerType)
        {
            return new FunctionType(SourceType.Argument, new FunctionType(InnerType, SourceType.Result));
        }

        /// <summary>
        /// Gets the type of "Argument" with the given source function type and inner type.
        /// </summary>
        public static FunctionType GetArgumentType(FunctionType SourceType, Type InnerType)
        {
            return new FunctionType(SourceType.Result, InnerType);
        }

        public override object Evaluate(object Argument)
        {
            object func = this._Function.Evaluate(Argument);
            object arg = this._Argument.Evaluate(Argument);
            return ((Function)func).Evaluate(arg);
        }

        public static ISerialization<Function> GetSerialization(FunctionType Type)
        {
            return new _Serialization
            {
                SourceType = Type
            };
        }

        private class _Serialization : ISerialization<Function>
        {
            public void Write(Function Object, OutStream Stream)
            {
                CallFunction cf = (CallFunction)Object;
                Type.Write(cf._InnerType, Stream);
                GetFunctionType(this.SourceType, cf._InnerType).Write(cf._Function, Stream);
                GetArgumentType(this.SourceType, cf._InnerType).Write(cf._Argument, Stream);
            }

            Function ISerialization<Function>.Read(InStream Stream)
            {
                Type innertype = Type.Read(Stream);
                Function function = GetFunctionType(this.SourceType, innertype).Read(Stream);
                Function argument = GetArgumentType(this.SourceType, innertype).Read(Stream);
                return new CallFunction(innertype, function, argument);
            }

            Maybe<long> ISerialization<Function>.Size
            {
                get
                {
                    return Maybe<long>.Nothing;
                }
            }

            public FunctionType SourceType;
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
    public sealed class FunctionType : Type, ISerialization<Function>, ISerialization<object>
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
        public static bool KindEquals(FunctionType A, FunctionType B)
        {
            return
                Type.Equal(A._Argument, B._Argument) &&
                Type.Equal(A._Result, B._Result);
        }

        public static bool KindEquals(Type A, Type B)
        {
            return KindEquals((FunctionType)A, (FunctionType)B);
        }

        public static ISerialization<Type> KindSerialization
        {
            get
            {
                return _KindSerialization.Instance;
            }
        }

        private sealed class _KindSerialization :  ISerialization<Type>
        {
            public static _KindSerialization Instance = new _KindSerialization();

            void ISerialization<Type>.Write(Type Object, OutStream Stream)
            {
                FunctionType ft = (FunctionType)Object;
                Type.Write(ft._Argument, Stream);
                Type.Write(ft._Result, Stream);
            }

            Type ISerialization<Type>.Read(InStream Stream)
            {
                return new FunctionType(Type.Read(Stream), Type.Read(Stream));
            }

            public Maybe<long> Size
            {
                get
                {
                    return Maybe<long>.Nothing;
                }
            }
        }

        public override ISerialization<object> Serialization
        {
            get
            {
                return this;
            }
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

        public void Write(Function Object, OutStream Stream)
        {
            FunctionKind kind = FunctionKind.ForType(Object.GetType());
            Stream.WriteByte(kind.ID);
            kind.GetSerialization(this).Write(Object, Stream);
        }

        public new Function Read(InStream Stream)
        {
            FunctionKind kind = FunctionKind.ForID(Stream.ReadByte(), this);
            return kind.GetSerialization(this).Read(Stream);
        }

        void ISerialization<object>.Write(object Object, OutStream Stream)
        {
            Function func = (Function)Object;
            this.Write(func, Stream);
        }

        object ISerialization<object>.Read(InStream Stream)
        {
            return this.Read(Stream);
        }

        public Maybe<long> Size
        {
            get
            {
                return Maybe<long>.Nothing;
            }
        }

        private Type _Argument;
        private Type _Result;
    }
}