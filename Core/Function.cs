using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Contains symbols and definitions related to functions.
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// A function that takes two types and constructs a type for a function that takes a value of the first type and
        /// returns a value of the second. Unlike the full type constructor, this can not be used to create a function whose result type depends on
        /// the argument. This is the symbol for the "->" operator.
        /// </summary>
        /// <remarks>The type of this symbol is "type -> type -> type"</remarks>
        public static readonly Symbol SimpleTypeConstructor;

        /// <summary>
        /// A function that constructs a function type whose result type may vary with the argument value. This is the symbol
        /// for the "=>" pseudo-operator.
        /// </summary>
        /// <remarks>The type of this symbol is "type a => (a -> type) -> type</remarks>
        public static readonly Symbol TypeConstructor;

        /// <summary>
        /// Constructs the type for a function with the given argument and result types.
        /// </summary>
        public static Expression Type(Expression ArgumentType, Expression ResultType)
        {
            return SimpleTypeConstructor + ArgumentType + ResultType;
        }

        /// <summary>
        /// Constructs the type for a function whose result type may vary based on the argument (given as a symbol which may be used in the result type).
        /// </summary>
        public static Expression Type(Symbol Argument, Expression ResultType)
        {
            return TypeConstructor + Argument.Type + (Argument - ResultType);
        }

        /// <summary>
        /// Defines relations involving functions in the given scope.
        /// </summary>
        public static void Define(Scope Scope)
        {
            Scope.DefineRule(3, new ResultTypeExpression(SimpleTypeConstructor + Term.Get(0) + Term.Get(1), Term.Get(2)), Term.Get(1));
            Scope.DefineRule(3, new ResultTypeExpression(TypeConstructor + Term.Get(0) + Term.Get(1), Term.Get(2)), Term.Get(1) + Term.Get(2));
        }

        static Function()
        {
            Expression reflexivetype = Expression.ReflexiveType;

            SimpleTypeConstructor = new Symbol("->");
            SimpleTypeConstructor.SetType(Type(reflexivetype, Type(reflexivetype, reflexivetype)));

            Symbol argumenttype = new Symbol(reflexivetype);
            TypeConstructor = new Symbol("=>");
            TypeConstructor.SetType(Type(argumenttype, Type(Type(argumenttype, reflexivetype), reflexivetype)));
        }
    }

    /// <summary>
    /// An expression defined by a function call.
    /// </summary>
    public class CallExpression : Expression
    {
        public CallExpression(Expression Function, Expression Argument)
        {
            this._Function = Function;
            this._Argument = Argument;
        }

        /// <summary>
        /// Gets the expression that defines the function for this call.
        /// </summary>
        public Expression Function
        {
            get
            {
                return this._Function;
            }
        }

        /// <summary>
        /// Gets the expression that defines the argument for this call.
        /// </summary>
        public Expression Argument
        {
            get
            {
                return this._Argument;
            }
        }

        public override Expression Type
        {
            get 
            {
                return new ResultTypeExpression(this._Function.Type, this._Argument);
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            Expression nfunc = this._Function.Fill(Terms);
            Expression narg = this._Argument.Fill(Terms);
            if (this._Function != nfunc || this._Argument != narg)
            {
                return new CallExpression(nfunc, narg);
            }
            return this;
        }

        private Expression _Function;
        private Expression _Argument;
    }

    /// <summary>
    /// An expression whose value is the type of the result of a function call with a function of a certain type and
    /// a given argument.
    /// </summary>
    public class ResultTypeExpression : Expression
    {
        public ResultTypeExpression(Expression FunctionType, Expression Argument)
        {
            this._FunctionType = FunctionType;
            this._Argument = Argument;
        }

        /// <summary>
        /// Gets the type of the function for the function call.
        /// </summary>
        public Expression FunctionType
        {
            get
            {
                return this._FunctionType;
            }
        }

        /// <summary>
        /// Gets the argument for the function call.
        /// </summary>
        public Expression Argument
        {
            get
            {
                return this._Argument;
            }
        }

        public override Expression Type
        {
            get
            {
                return ReflexiveType;
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            Expression nfunctype = this._FunctionType.Fill(Terms);
            Expression narg = this._Argument.Fill(Terms);
            if (this._FunctionType != nfunctype || this._Argument != narg)
            {
                return new ResultTypeExpression(nfunctype, narg);
            }
            return this;
        }

        private Expression _FunctionType;
        private Expression _Argument;
    }

    /// <summary>
    /// An expression that defines a function whose result when called is defined by an expression where one symbol
    /// takes the place of the argument.
    /// </summary>
    public class LambdaExpression : Expression
    {
        public LambdaExpression(Symbol Argument, Expression Inner)
        {
            this._Argument = Argument;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the argument for the lambda expression.
        /// </summary>
        public Symbol Argument
        {
            get
            {
                return this._Argument;
            }
        }

        /// <summary>
        /// Gets the inner expression for the lambda expression.
        /// </summary>
        public Expression Inner
        {
            get
            {
                return this._Inner;
            }
        }

        public override Expression Type
        {
            get
            {
                return Function.Type(this._Argument, this._Inner.Type);
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            Expression ninner = this._Inner.Fill(Terms);
            if (this._Inner != ninner)
            {
                return new LambdaExpression(this._Argument, ninner);
            }
            return this;
        }

        private Symbol _Argument;
        private Expression _Inner;
    }
}