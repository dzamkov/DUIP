﻿using System;
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
        public abstract bool Set(TKey Key, T Value);

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
                this.Set(Key, value);
            }
        }
    }

    /// <summary>
    /// A map where only some keys have values.
    /// </summary>
    public abstract class PartialMap<TKey, T> : Map<TKey, Maybe<T>>
    {

    }

    /// <summary>
    /// An implementation of a hashed map contained within mutable data.
    /// </summary>
    public class HashMap<TKey, T> : PartialMap<TKey, T>
    {
        public HashMap(Data Data, ISerialization<TKey> KeySerialization, ISerialization<T> ValueSerialization, IHashing<TKey> KeyHashing)
        {
            this._KeySerialization = KeySerialization;
            this._ValueSerialization = ValueSerialization;
            this._KeyHashing = KeyHashing;
        }

        public override Maybe<T> Lookup(TKey Key)
        {
            throw new NotImplementedException();
        }

        public override bool Set(TKey Key, Maybe<T> Value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a hashmap with the given allocator.
        /// </summary>
        /// <param name="Buckets">The maximum amount of items the hash map can store. Proportional to the size of the required data.</param>
        /// <param name="CellarBuckets">The amount of buckets to be used exclusively in the case of hash collisions.</param>
        public static HashMap<TKey, T> Create<TRef>(
            Allocator<TRef> Allocator,
            long Buckets,
            long CellarBuckets,
            ISerialization<TKey> KeySerialization, 
            ISerialization<T> ValueSerialization, 
            IHashing<TKey> KeyHashing,
            out TRef Ref)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Header for a hashmap in data.
        /// </summary>
        public struct Header
        {
            /// <summary>
            /// The amount of buckets in the hashmap.
            /// </summary>
            public long Buckets;

            /// <summary>
            /// The amount of cellar buckets preceding the hashmap.
            /// </summary>
            public long CellarBuckets;

            /// <summary>
            /// The index of the first bucket with no contents.
            /// </summary>
            public long FirstFreeBucket;

            /// <summary>
            /// The amount of items in the hashmap.
            /// </summary>
            public long Items;
        }

        /// <summary>
        /// Gets the serialization method used for keys. This serialization must have
        /// a fixed size.
        /// </summary>
        public ISerialization<TKey> KeySerialization
        {
            get
            {
                return this._KeySerialization;
            }
        }

        /// <summary>
        /// Gets the serialization method used for values. This serialization must have
        /// a fixed size.
        /// </summary>
        public ISerialization<T> ValueSerialization
        {
            get
            {
                return this._ValueSerialization;
            }
        }

        /// <summary>
        /// Gets the method used for comparing and hashing keys.
        /// </summary>
        public IHashing<TKey> KeyHashing
        {
            get
            {
                return this._KeyHashing;
            }
        }

        private ISerialization<TKey> _KeySerialization;
        private ISerialization<T> _ValueSerialization;
        private IHashing<TKey> _KeyHashing;
    }
}