using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Describes a method of ordering items of a certain type.
    /// </summary>
    /// <typeparam name="T">The common base type for orderable objects.</typeparam>
    public interface IOrdering<T>
    {
        /// <summary>
        /// Gets the relationship A has to B in the ordering.
        /// </summary>
        Relation Compare(T A, T B);
    }

    /// <summary>
    /// The type of relationship two items can have in an ordering.
    /// </summary>
    public enum Relation
    {
        Lesser,
        Equal,
        Greater,
    }
}