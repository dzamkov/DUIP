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
}