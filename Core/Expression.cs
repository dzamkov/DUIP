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

        public static LambdaExpression operator -(Symbol Argument, Expression Inner)
        {
            return new LambdaExpression(Argument, Inner);
        }
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