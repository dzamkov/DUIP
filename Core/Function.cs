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
        public static CallFunction Call(Function Function, Function Argument)
        {
            return new CallFunction(Function, Argument);
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
    public sealed class CallFunction : Function
    {
        public CallFunction(Function Function, Function Argument)
        {
            this._Function = Function;
            this._Argument = Argument;
        }

        /// <summary>
        /// Get a function that, when evaluated with the argument of this function, will return the function
        /// component of this call.
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
        /// component of this call.
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
}