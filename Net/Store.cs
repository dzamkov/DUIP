using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An indexed collection of data.
    /// </summary>
    /// <typeparam name="T">A reference to a data in the store.</typeparam>
    public abstract class Store<T>
    {
        /// <summary>
        /// Looks up a datum with the given reference, or returns null if the datum is not found.
        /// </summary>
        public abstract Query<Data> Lookup(T Reference);
    }

    /// <summary>
    /// A store that can accept new datums.
    /// </summary>
    public abstract class Cache<T> : Store<T>
    {
        /// <summary>
        /// Stores a piece of data in the cache.
        /// </summary>
        public abstract Query<Void> Set(T Reference, Data Data);
    }
}