using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A cache for indexed data stored in an allocator that keeps track of how often items are used to allow them
    /// to automatically be removed when needed.
    /// </summary>
    /// <typeparam name="TRef">A reference to data in the cache.</typeparam>
    /// <typeparam name="TPtr">A pointer to data in the allocator.</typeparam>
    public class Cache<TRef, TPtr> : IHandle
    {
        private Cache()
        {

        }

        /// <summary>
        /// Creates a new cache with the given allocator.
        /// </summary>
        /// <param name="InitialSize">The initial size to use for the hashmap in the cache.</param>
        public static Cache<TRef, TPtr> Create(
            Allocator<TPtr> Allocator, 
            ISerialization<TPtr> PointerSerialization, 
            ISerialization<TRef> ReferenceSerialization,
            IHashing<TRef> ReferenceHashing,
            long InitialSize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the allocator this cache uses.
        /// </summary>
        public Allocator<TPtr> Allocator
        {
            get
            {
                return this._Scheme.Allocator;
            }
        }

        public Data Source
        {
            get
            {
                return this._Source;
            }
        }

        public CacheScheme<TRef, TPtr> Scheme
        {
            get
            {
                return this._Scheme;
            }
        }

        private Data _Source;
        private CacheScheme<TRef, TPtr> _Scheme;
        private HashMap<TRef, TPtr> _PrimaryHashMap;
        private HashMap<TRef, TPtr> _SecondaryHashMap;
        private TPtr _PrimaryHashMapPtr;
        private TPtr _SecondaryHashMapPtr;
        private TRef _Last;
    }

    /// <summary>
    /// Scheme information for a cache.
    /// </summary>
    public class CacheScheme<TRef, TPtr>
    {
        /// <summary>
        /// The allocator used to store and retreive data.
        /// </summary>
        public Allocator<TPtr> Allocator;

    }
}