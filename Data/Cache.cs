﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A cache for indexed data stored in an allocator.
    /// </summary>
    /// <typeparam name="TRef">A reference to data in the cache.</typeparam>
    /// <typeparam name="TPtr">A pointer to data in the allocator.</typeparam>
    public class Cache<TRef, TPtr> : Map<TRef, Data>
    {

        private Allocator<TPtr> _Allocator;
    }
}