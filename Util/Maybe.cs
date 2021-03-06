﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A variant type that may or may not have a value of a nested type. If the structure has no value of the type it is itself a "Nothing" value.
    /// </summary>
    public struct Maybe<T>
    {
        /// <summary>
        /// Gets a maybe value with the given nested value.
        /// </summary>
        public static Maybe<T> Just(T Value)
        {
            return new Maybe<T>()
            {
                Value = Value,
                HasValue = true
            };
        }

        /// <summary>
        /// Gets a maybe value with no nested value.
        /// </summary>
        public static Maybe<T> Nothing
        {
            get
            {
                return new Maybe<T>() { HasValue = false };
            }
        }

        /// <summary>
        /// Gets if this maybe value is the "Nothing" value.
        /// </summary>
        public bool IsNothing
        {
            get
            {
                return !this.HasValue;
            }
        }

        public static implicit operator Maybe<T>(T Value)
        {
            return Maybe<T>.Just(Value);
        }

        /// <summary>
        /// Tries getting the nested value for this maybe value.
        /// </summary>
        public bool TryGetValue(out T Value)
        {
            Value = this.Value;
            return this.HasValue;
        }

        /// <summary>
        /// Gets the nested value of the maybe value if there is one, or throws an exception. This should be used
        /// when the value is known to not be nothing.
        /// </summary>
        public T OrExcept
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new Exception();
                }
                return this.Value;
            }
        }

        /// <summary>
        /// Gets the nested value of the maybe value if there is one, or returns the given default value.
        /// </summary>
        public T OrDefault(T Default)
        {
            if (this.HasValue)
            {
                return this.Value;
            }
            else
            {
                return Default;
            }
        }

        /// <summary>
        /// The nested value of this maybe value, if this maybe value has a nested value.
        /// </summary>
        public T Value;

        /// <summary>
        /// Gets if this maybe value has a value of the nested type.
        /// </summary>
        public bool HasValue;
    }
}