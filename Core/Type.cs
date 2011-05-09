using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interpretation of values (called instances) within a certain set.
    /// </summary>
    /// <typeparam name="T">A representation of an instance of the type.</typeparam>
    public abstract class Type<T> : Type
    {
        /// <summary>
        /// Gets if the two representations of values in the type represent the same value.
        /// </summary>
        public abstract bool Equal(T A, T B);

        /// <summary>
        /// Merges the change from Base to A and Base to B and applies them to Base, taking into
        /// account the intentions of the type. Returns nothing if a logical merging is not possible or attempted.
        /// </summary>
        public virtual Maybe<T> Merge(T Base, T A, T B)
        {
            if (this.Equal(Base, A))
            {
                return B;
            }
            if (this.Equal(Base, B))
            {
                return A;
            }
            return Maybe<T>.Nothing;
        }
    }

    /// <summary>
    /// The generalized form of a type with no specific instance type.
    /// </summary>
    public class Type
    {

    }
}