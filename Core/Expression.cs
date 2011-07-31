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
        /// Replaces the terms in the expression with their corresponding values in the given array.
        /// </summary>
        public abstract Expression Fill(Expression[] Terms);

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
}