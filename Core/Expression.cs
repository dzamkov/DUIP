using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A relation of values and variables that results in a value.
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        /// Gets an expression (using the same variable set as this expression) that represents the type
        /// of this expression.
        /// </summary>
        public abstract Expression Type { get; }

        /// <summary>
        /// Gets the reflexive type.
        /// </summary>
        public static Symbol ReflexiveType
        {
            get
            {
                return DUIP.Type.Reflexive;
            }
        }

        public static CallExpression operator +(Expression Function, Expression Argument)
        {
            return new CallExpression(Function, Argument);
        }

        public static LambdaExpression operator -(LambdaArgument Argument, Expression Inner)
        {
            return new LambdaExpression(Argument, Inner);
        }
    }

    /// <summary>
    /// An expression representing a constant value defined by the program. Symbol that have equivalent references
    /// are considered to have an equivalent value.
    /// </summary>
    public sealed class Symbol : Expression
    {
        public Symbol(int ID)
        {
            this._ID = ID;
        }

        public Symbol()
        {

        }

        public Symbol(Expression Type, int ID)
            : this(ID)
        {
            this._Type = Type;
        }

        public Symbol(Expression Type)
        {
            this._Type = Type;
        }

        public override Expression Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// Sets the type of this symbol. This may only be called once, and can only be called if a type was not included
        /// in the constructor for the symbol.
        /// </summary>
        public void SetType(Expression Type)
        {
            this._Type = Type;
        }

        /// <summary>
        /// Gets an optional identifier for this symbol for use in serialization.
        /// </summary>
        public int ID
        {
            get
            {
                return this._ID;
            }
        }

        private Expression _Type;
        private int _ID;
    }

    /// <summary>
    /// An expression that takes the value of a variable.
    /// </summary>
    public sealed class VariableExpression : Expression
    {
        public VariableExpression(int Index)
        {
            this._Index = Index;
        }

        /// <summary>
        /// Gets the index of the variable this expression refers to.
        /// </summary>
        public int Index
        {
            get
            {
                return this._Index;
            }
        }

        public override Expression Type
        {
            get
            {
                return new VariableTypeExpression(this._Index);
            }
        }

        private int _Index;
    }

    /// <summary>
    /// An expression that takes the type of a variable.
    /// </summary>
    public sealed class VariableTypeExpression : Expression
    {
        public VariableTypeExpression(int Index)
        {
            this._Index = Index;
        }

        /// <summary>
        /// Gets the index of the variable this expression refers to.
        /// </summary>
        public int Index
        {
            get
            {
                return this._Index;
            }
        }

        public override Expression Type
        {
            get
            {
                return ReflexiveType;
            }
        }

        private int _Index;
    }
}