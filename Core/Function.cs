using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Defines a relation between an argument and a result.
    /// </summary>
    public class Function
    {

    }

    /// <summary>
    /// A type for a relation (or rather, a specific definition of a relation) between an argument and a result of certain types.
    /// </summary>
    public class FunctionType : Type<Function>
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

        public override bool Equal(Function A, Function B)
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

        /// <summary>
        /// Evaluates a given function (of this type) for a given argument.
        /// </summary>
        public TRes Evaluate<TArg, TRes>(Function Function, TArg Argument)
        {
            throw new NotImplementedException();
        }

        private Type _Argument;
        private Type _Result;
    }   
}