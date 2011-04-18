using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Describes a method of determining wether two objects of a common type are "equal".
    /// </summary>
    /// <typeparam name="T">The common base type for equatable objects.</typeparam>
    public interface IEquality<T>
    {
        /// <summary>
        /// Gets wether the two given objects are equal.
        /// </summary>
        bool Equal(T A, T B);
    }

    /// <summary>
    /// Describes a method of creating a "hash" in to form of a BigInt for an object of a certain type. Two objects with the same hash are guranteed
    /// to be equal (however, the inverse isn't true).
    /// </summary>
    public interface IHashing<T> : IEquality<T>
    {
        /// <summary>
        /// Gets the hash of an object.
        /// </summary>
        BigInt Hash(T Object);
    }
}