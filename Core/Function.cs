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
        /// Constructs the type for a function whose result type may vary based on the argument.
        /// </summary>
        public static Expression Type(LambdaArgument Argument, Expression ResultType)
        {
            return TypeConstructor + Argument.Type + (Argument - ResultType);
        }

        static Function()
        {
            Expression reflexivetype = Expression.ReflexiveType;

            SimpleTypeConstructor = new Symbol(0x00010000);
            SimpleTypeConstructor.SetType(Type(reflexivetype, Type(reflexivetype, reflexivetype)));

            TypeConstructor = new Symbol(0x00010001);
            TypeConstructor.SetType(Type(new LambdaArgument(0, reflexivetype), Type(Type(new VariableExpression(0), reflexivetype), reflexivetype)));
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

        private Expression _FunctionType;
        private Expression _Argument;
    }

    /// <summary>
    /// An expression that defines a function whose result when called is defined by an expression where one variable
    /// takes the place of the argument.
    /// </summary>
    public class LambdaExpression : Expression
    {
        public LambdaExpression(LambdaArgument Argument, Expression Inner)
        {
            this._Argument = Argument;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the argument for the lambda expression.
        /// </summary>
        public LambdaArgument Argument
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

        private LambdaArgument _Argument;
        private Expression _Inner;
    }

    /// <summary>
    /// Identifies and describes a variable to be used as the argument in a lambda expression.
    /// </summary>
    public struct LambdaArgument
    {
        public LambdaArgument(int Index, Expression Type)
        {
            this.Index = Index;
            this.Type = Type;
        }

        /// <summary>
        /// The index of the variable to be used as the argument. If this variable is already defined in the scope of the
        /// lambda expression, it will be replaced when evaluating the inner expression.
        /// </summary>
        public int Index;

        /// <summary>
        /// The type of the variable to be used as the argument.
        /// </summary>
        public Expression Type;
    }
}