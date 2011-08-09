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
            this.Function = Function;
            this.Argument = Argument;
        }

        /// <summary>
        /// The expression that defines the function for this call.
        /// </summary>
        public readonly Expression Function;

        /// <summary>
        /// The expression that defines the argument for this call.
        /// </summary>
        public readonly Expression Argument;

        public override Expression Type
        {
            get 
            {
                return new ResultTypeExpression(this.Function.Type, this.Argument);
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            Expression nfunc = this.Function.Fill(Terms);
            Expression narg = this.Argument.Fill(Terms);
            if (this.Function != nfunc || this.Argument != narg)
            {
                return new CallExpression(nfunc, narg);
            }
            return this;
        }
    }

    /// <summary>
    /// An expression whose value is the type of the result of a function call with a function of a certain type and
    /// a given argument.
    /// </summary>
    public class ResultTypeExpression : Expression
    {
        public ResultTypeExpression(Expression FunctionType, Expression Argument)
        {
            this.FunctionType = FunctionType;
            this.Argument = Argument;
        }

        /// <summary>
        /// The type of the function for the function call.
        /// </summary>
        public readonly Expression FunctionType;

        /// <summary>
        /// The argument for the function call.
        /// </summary>
        public readonly Expression Argument;

        public override Expression Type
        {
            get
            {
                return ReflexiveType;
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            Expression nfunctype = this.FunctionType.Fill(Terms);
            Expression narg = this.Argument.Fill(Terms);
            if (this.FunctionType != nfunctype || this.Argument != narg)
            {
                return new ResultTypeExpression(nfunctype, narg);
            }
            return this;
        }
    }

    /// <summary>
    /// An expression that defines a function whose result when called is defined by an expression where one symbol
    /// takes the place of the argument.
    /// </summary>
    public class LambdaExpression : Expression
    {
        public LambdaExpression(Symbol Argument, Expression Inner)
        {
            this.Argument = Argument;
            this.Inner = Inner;
        }

        /// <summary>
        /// The argument for the lambda expression.
        /// </summary>
        public readonly Symbol Argument;

        /// <summary>
        /// The inner expression for the lambda expression.
        /// </summary>
        public readonly Expression Inner;

        public override Expression Type
        {
            get
            {
                return Function.Type(this.Argument, this.Inner.Type);
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            Expression ninner = this.Inner.Fill(Terms);
            if (this.Inner != ninner)
            {
                return new LambdaExpression(this.Argument, ninner);
            }
            return this;
        }
    }
}