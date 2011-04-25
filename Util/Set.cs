using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A immutable unordered collection of items where each item can appear at most once. Alternatively, this
    /// can be regarded as a mapping of all items to a boolean value indicating wether they are in the set.
    /// </summary>
    public abstract class Set<T> : Map<T, bool>
    {
        /// <summary>
        /// Tries getting an iterator for the items in the set, or returns null if this is not
        /// possible. The items may be given in any order.
        /// </summary>
        public virtual IEnumerable<T> Items
        {
            get
            {
                return null;
            }
        }
    }
}