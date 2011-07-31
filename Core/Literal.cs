using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An expression whose value can be directly interpreted by the program.
    /// </summary>
    /// <typeparam name="T">The program-defined type for the value.</typeparam>
    public class Literal<T> : Expression
    {
        public Literal(T Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// The type that all instances of this literal type will have.
        /// </summary>
        public static Expression InstanceType;

        /// <summary>
        /// The function that determines wether two values of this literal type are equal.
        /// </summary>
        public static Func<T, T, bool> Equal;

        /// <summary>
        /// Gets the value of this literal.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public override Expression Type
        {
            get
            {
                return InstanceType;
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            return this;
        }

        private T _Value;
    }
}