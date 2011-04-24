using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A cache for indexed data stored in an allocator.
    /// </summary>
    /// <typeparam name="TRef">A reference to data in the cache.</typeparam>
    /// <typeparam name="TPtr">A pointer to data in the allocator.</typeparam>
    public class Cache<TRef, TPtr> : PartialMap<TRef, Data>
    {
        public override Maybe<Data> Lookup(TRef Key)
        {
            throw new NotImplementedException();
        }

        public override bool Immutable
        {
            get
            {
                return false;
            }
        }

        private Allocator<TPtr> _Allocator;
        private ISerialization<TPtr> _PointerSerialization;
        private ISerialization<TRef> _ReferenceSerialization;
    }
}