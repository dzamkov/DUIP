using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Memory
{
    /// <summary>
    /// An interface to a mutable mapping structure.
    /// </summary>
    public abstract class Map<TKey, T>
    {
        /// <summary>
        /// Tries setting the value for a key. Returns true on success (looking up the key
        /// will return the new value) or false if the operation is not possible.
        /// </summary>
        public abstract bool Set(TKey Key, T Value);

        /// <summary>
        /// Gets the value for a key.
        /// </summary>
        public abstract T Get(TKey Key);
    }
}