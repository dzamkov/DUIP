using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An instance of any type.
    /// </summary>
    public class Any<T> : Any
    {
        public Any(Type<T> Type, T Value)
        {
            this._Type = Type;
            this._Value = Value;
        }

        /// <summary>
        /// Gets the type this is an instance of.
        /// </summary>
        public Type<T> Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// The value of this when interpreted with the associated type.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public override Type GeneralType
        {
            get
            {
                return this._Type;
            }
        }

        private Type<T> _Type;
        private T _Value;
    }

    /// <summary>
    /// Generalized form of Any.
    /// </summary>
    public abstract class Any
    {
        /// <summary>
        /// Creates a instance of the AnyType.
        /// </summary>
        public static Any<T> Create<T>(Type<T> Type, T Value)
        {
            return new Any<T>(Type, Value);
        }

        /// <summary>
        /// Gets the general form of the type for the instance.
        /// </summary>
        public abstract Type GeneralType { get; }
    }

    /// <summary>
    /// A type whose instances include any instance of any type.
    /// </summary>
    public class AnyType : Type<Any>
    {
        private AnyType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly AnyType Singleton = new AnyType();
    }
}