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
    public class Cache<TRef, TPtr>
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
            long InitialSize)
        {

        }

        /// <summary>
        /// Gets the allocator this cache uses.
        /// </summary>
        public Allocator<TPtr> Allocator
        {
            get
            {
                return this._Allocator;
            }
        }

        /// <summary>
        /// Gets the pointer serialization method this cache uses. This serialization
        /// must have a fixed size.
        /// </summary>
        public ISerialization<TPtr> PointerSerialization
        {
            get
            {
                return this._PointerSerialization;
            }
        }

        /// <summary>
        /// Gets the reference serialization method this cache uses. This serialization
        /// must have a fixed size.
        /// </summary>
        public ISerialization<TRef> ReferenceSerialization
        {
            get
            {
                return this._ReferenceSerialization;
            }
        }

        private Allocator<TPtr> _Allocator;
        private ISerialization<TPtr> _PointerSerialization;
        private ISerialization<TRef> _ReferenceSerialization;
        private TPtr _Primary;
        private TPtr _Secondary;
        private TRef _Last;
    }
}