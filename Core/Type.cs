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
    }

    /// <summary>
    /// The generalized form of a type with no specific instance type.
    /// </summary>
    public class Type
    {

    }
}