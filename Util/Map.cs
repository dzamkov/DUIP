using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An immutable mapping of keys to values.
    /// </summary>
    public abstract class Map<TKey, T>
    {
        /// <summary>
        /// Looks up the value for the given key.
        /// </summary>
        public abstract T Lookup(TKey Key);

        /// <summary>
        /// Gets the value for a key.
        /// </summary>
        public T this[TKey Key]
        {
            get
            {
                return this.Lookup(Key);
            }
        }

        /// <summary>
        /// Applies a mapping to all items in an enumerable sequence.
        /// </summary>
        public IEnumerable<T> Apply(IEnumerable<TKey> Source)
        {
            return
                from k in Source
                select this.Lookup(k);
        }
    }

    /// <summary>
    /// A map created from a function delegate.
    /// </summary>
    public sealed class FuncMap<TKey, T> : Map<TKey, T>
    {
        public FuncMap(Func<TKey, T> Func)
        {
            this._Func = Func;
        }

        /// <summary>
        /// Gets the mapping function used.
        /// </summary>
        public Func<TKey, T> Func
        {
            get
            {
                return this._Func;
            }
        }

        public override T Lookup(TKey Key)
        {
            return this._Func(Key);
        }

        private Func<TKey, T> _Func;
    }

    /// <summary>
    /// A map where only some keys have values.
    /// </summary>
    public abstract class PartialMap<TKey, T> : Map<TKey, Maybe<T>>
    {
        /// <summary>
        /// Tries getting an iterator for the non-nothing values in this map, or returns null
        /// if this is not possible. The items may be given in any order.
        /// </summary>
        public virtual IEnumerable<KeyValuePair<TKey, T>> Items
        {
            get
            {
                return null;
            }
        }
    }
}