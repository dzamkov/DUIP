using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A mapping of keys to values.
    /// </summary>
    public abstract class Map<TKey, T>
    {
        /// <summary>
        /// Looks up the value for the given key.
        /// </summary>
        public abstract T Lookup(TKey Key);

        /// <summary>
        /// Tries setting the value associated with a key. Returns true on success (Looking up the key will return
        /// the new value) or false on failure.
        /// </summary>
        public abstract bool Modify(TKey Key, T Value);

        /// <summary>
        /// Gets if this map is immutable, immutable maps can not have the values for their keys changed.
        /// </summary>
        public virtual bool Immutable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the value for a key.
        /// </summary>
        public T this[TKey Key]
        {
            get
            {
                return this.Lookup(Key);
            }
            set
            {
                this.Modify(Key, value);
            }
        }
    }

    /// <summary>
    /// A map where only some keys have values.
    /// </summary>
    public abstract class PartialMap<TKey, T> : Map<TKey, Maybe<T>>
    {
        /// <summary>
        /// Tries removing the value associated with a key. Returns true on success (Looking up the key will return
        /// nothing) or false on failure.
        /// </summary>
        public bool Remove(TKey Key)
        {
            return this.Modify(Key, Maybe<T>.Nothing);
        }

        /// <summary>
        /// Tries getting an iterator for the non-nothing values in this map, or returns null
        /// if this is not possible. The items may be given in any order.
        /// </summary>
        public virtual IIterator<KeyValuePair<TKey, T>> Items
        {
            get
            {
                return null;
            }
        }
    }
}