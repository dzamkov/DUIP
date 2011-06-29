﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An implementation of a hashed map contained within mutable data.
    /// </summary>
    public class HashMap<TKey, T> : Map<TKey, Maybe<T>>, IHandle
    {
        private HashMap()
        {

        }

        public override Maybe<T> Get(TKey Key)
        {
            long bi; Bucket b;
            return this.Get(this._Scheme.KeyHashing.Hash(Key), Key, out bi, out b);
        }

        /// <summary>
        /// Looks up an item in the map.
        /// </summary>
        /// <param name="Hash">The hash of the key using the key hashing method for the hashmap.</param>
        /// <param name="BucketIndex">The index of the bucket in which the item was found.</param>
        /// <param name="Bucket">The contents for the bucket in which the item was found.</param>
        public Maybe<T> Get(BigInt Hash, TKey Key, out long BucketIndex, out Bucket Bucket)
        {
            BucketIndex = this.GetBucketIndex(Hash);
            while (true)
            {
                Bucket = this.GetBucket(BucketIndex);
                if (!Bucket.Free && this._Scheme.KeyHashing.Equal(Bucket.Key, Key))
                {
                    return Bucket.Value;
                }
                if (Bucket.Reference == BucketIndex)
                {
                    return Maybe<T>.Nothing;
                }
                BucketIndex = Bucket.Reference;
            }
        }


        public override bool Set(TKey Key, Maybe<T> Value)
        {
            long bi; Bucket b;
            return this.Set(this._Scheme.KeyHashing.Hash(Key), Key, Value, out bi, out b);
        }

        /// <summary>
        /// Sets, creates, or removes an item with the given key and value.
        /// </summary>
        /// <param name="Hash">The hash of the key using the key hashing method for the hashmap.</param>
        /// <param name="BucketIndex">The index of the bucket in which the item was set. This is undefined if the item was removed.</param>
        /// <param name="Bucket">The contents for the bucket in which the item was set. This is undefined if the item was removed.</param>
        public bool Set(BigInt Hash, TKey Key, Maybe<T> Value, out long BucketIndex, out Bucket Bucket)
        {
            BucketIndex = this.GetBucketIndex(Hash);
            Maybe<long> pbi = Maybe<long>.Nothing; // Index of the previous bucket in the chain

            while (true)
            {
                Bucket = this.GetBucket(BucketIndex);

                // Is this bucket the end of a chain?
                if (Bucket.Reference == BucketIndex)
                {
                    // Is it free?
                    if (Bucket.Free)
                    {
                        if (Value.HasValue)
                        {
                            // Add item to this bucket
                            Bucket.Free = false;
                            Bucket.Key = Key;
                            Bucket.Value = Value.Value;
                            this.SetBucket(BucketIndex, Bucket);
                            this._Add(BucketIndex);
                            this._UpdateHeader();
                            return true;
                        }
                        else
                        {
                            if (pbi.HasValue)
                            {
                                // There is no reason for this item to remain in a chain
                                Bucket pb = this.GetBucket(pbi.Value);
                                pb.Reference = pbi.Value;
                                this.SetBucket(pbi.Value, pb);
                            }

                            // Removing a nonexistant item
                            return true;
                        }
                    }
                    else
                    {
                        if (this._Scheme.KeyHashing.Equal(Bucket.Key, Key))
                        {
                            if (Value.HasValue)
                            {
                                // Modify the item
                                Bucket.Value = Value.Value;
                                this.SetBucket(BucketIndex, Bucket);
                                return true;
                            }
                            else
                            {
                                // Remove the item
                                Bucket.Free = true;
                                this.SetBucket(BucketIndex, Bucket);
                                this._Remove(BucketIndex);
                                this._UpdateHeader();

                                // Remove from chain
                                if (pbi.HasValue)
                                {
                                    Bucket pb = this.GetBucket(pbi.Value);
                                    pb.Reference = pbi.Value;
                                    this.SetBucket(pbi.Value, pb);
                                }
                                return true;
                            }
                        }
                        else
                        {
                            if (Value.HasValue)
                            {
                                // There is no item with this key in the hashmap, add one anywhere
                                if (this._CanAdd)
                                {
                                    long ni = Bucket.Reference = this._Add();
                                    Bucket nb = new Bucket()
                                    {
                                        Free = false,
                                        Key = Key,
                                        Value = Value.Value,
                                        Reference = ni
                                    };
                                    this.SetBucket(ni, nb);
                                    this.SetBucket(BucketIndex, Bucket); // Make sure to add this item to the chain
                                    Bucket = nb;
                                    BucketIndex = ni;
                                    this._UpdateHeader();
                                    return true;
                                }
                                return false;
                            }
                            else
                            {
                                // Removing a nonexistant item
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    // Not at the end of a chain
                    if (Bucket.Free)
                    {
                        // Free buckets should not be in chains
                        if (pbi.HasValue)
                        {
                            Bucket pb = this.GetBucket(pbi.Value);
                            long next = pb.Reference = Bucket.Reference;
                            this.SetBucket(pbi.Value, pb);

                            Bucket.Reference = BucketIndex;
                            this.SetBucket(BucketIndex, Bucket);

                            // Continue on in the chain
                            BucketIndex = next;
                            continue;
                        }
                    }
                    else
                    {
                        // Is this what we are looking for?
                        if (this._Scheme.KeyHashing.Equal(Bucket.Key, Key))
                        {
                            if (Value.HasValue)
                            {
                                // Modify the item
                                Bucket.Value = Value.Value;
                                this.SetBucket(BucketIndex, Bucket);
                                return true;
                            }
                            else
                            {
                                // Remove the item
                                Bucket.Free = true;
                                this.SetBucket(BucketIndex, Bucket);
                                this._Remove(BucketIndex);
                                this._UpdateHeader();

                                // Remove from chain
                                if (pbi.HasValue)
                                {
                                    Bucket pb = this.GetBucket(pbi.Value);
                                    pb.Reference = Bucket.Reference;
                                    this.SetBucket(pbi.Value, pb);
                                }
                                return true;
                            }
                        }
                    }

                    // Advance to the next item in the chain
                    BucketIndex = Bucket.Reference;
                    continue;
                }
            }
        }

        /// <summary>
        /// Removes the item at the bucket with the given bucket index.
        /// </summary>
        public void RemoveBucket(long BucketIndex, Bucket Bucket)
        {
            Bucket.Free = true;
            this.SetBucket(BucketIndex, Bucket);

            this._Remove(BucketIndex);
            this._UpdateHeader();
        }

        /// <summary>
        /// Gets the bucket that corresponds to the given hash.
        /// </summary>
        public long GetBucketIndex(BigInt Hash)
        {
            Scheme scheme = this._Scheme;
            long hashbuckets = scheme.Buckets - scheme.CellarBuckets;
            return (long)((Hash % (ulong)hashbuckets).ToULong() + (ulong)scheme.CellarBuckets);
        }

        /// <summary>
        /// Gets the bucket whose hash corresponds with the given key.
        /// </summary>
        public long GetBucketIndex(TKey Key)
        {
            return this.GetBucketIndex(this._Scheme.KeyHashing.Hash(Key));
        }

        /// <summary>
        /// Retrieves the given bucket.
        /// </summary>
        public Bucket GetBucket(long BucketIndex)
        {
            Bucket b;
            using (Disposable<InStream> str = this._Source.Read(this.GetBucketOffset(BucketIndex)))
            {
                b = this._Scheme.BucketSerialization.Deserialize(str);
            }
            return b;
        }

        /// <summary>
        /// Sets the given bucket.
        /// </summary>
        public void SetBucket(long BucketIndex, Bucket Value)
        {
            using (Disposable<OutStream> str = this._Source.Write(this.GetBucketOffset(BucketIndex)))
            {
                this._Scheme.BucketSerialization.Serialize(Value, str);
            }
        }

        /// <summary>
        /// Searches for the first free or filled (depending on the Free parameter) bucket after the given bucket. The search 
        /// will wrap around to the beginning of the hashmap if needed. This method will not exit unless a suitable bucket is found.
        /// </summary>
        public long SearchBucketIndex(long Start, bool Free)
        {
            Scheme scheme = this._Scheme;
            Start++;
            long tb = scheme.Buckets;
            long bs = scheme.BucketSerialization.Size.OrExcept;
            while (true)
            {
                if (Start == tb)
                {
                    Start = 0;
                }
                using (Disposable<InStream> str = this._Source.Read(this.GetBucketOffset(Start)))
                {
                    while (Start < tb)
                    {
                        Bucket b = scheme.BucketSerialization.Deserialize(str);
                        if (b.Free == Free)
                        {
                            return Start;
                        }
                        Start++;
                    }
                }
                Start = 0;
            }
        }

        /// <summary>
        /// Gets the location of the given bucket in the memory for the map.
        /// </summary>
        public long GetBucketOffset(long Bucket)
        {
            return Header.Size + this._Scheme.BucketSerialization.Size.OrExcept * Bucket;
        }

        /// <summary>
        /// Gets if a new item can be added.
        /// </summary>
        private bool _CanAdd
        {
            get
            {
                return this._Header.Items < this._Scheme.Buckets;
            }
        }

        /// <summary>
        /// Gets a free bucket that can be used to add an item. Modifies (but does not update) the header to reflect
        /// this bucket as used.
        /// </summary>
        private long _Add()
        {
            this._Header.Items++;
            long ind = this._Header.FirstFreeBucket;
            if (this._Header.Items < this._Scheme.Buckets)
            {
                this._Header.FirstFreeBucket = this.SearchBucketIndex(ind, true);
            }
            if (this._Header.Items == 0)
            {
                this._Header.FirstFilledBucket = ind;
            }
            return ind;
        }

        /// <summary>
        /// Modifies (but does not update) the header to reflect the given bucket as used.
        /// </summary>
        private void _Add(long BucketIndex)
        {
            this._Header.Items++;
            if (BucketIndex == this._Header.FirstFreeBucket && this._Header.Items < this._Scheme.Buckets)
            {
                this._Header.FirstFreeBucket = this.SearchBucketIndex(BucketIndex, true);
            }
            if (this._Header.Items == 0)
            {
                this._Header.FirstFilledBucket = BucketIndex;
            }
        }

        /// <summary>
        /// Modifies (but does not update) the header to reflect the given bucket as free.
        /// </summary>
        private void _Remove(long BucketIndex)
        {
            this._Header.Items--;
            if (this._Header.FirstFilledBucket == BucketIndex && this._Header.Items > 0)
            {
                this._Header.FirstFilledBucket = this.SearchBucketIndex(BucketIndex, false);
            }
        }

        /// <summary>
        /// Updates the header in the memory for the hashmap with the header stored in this interface.
        /// </summary>
        private void _UpdateHeader()
        {
            using (Disposable<OutStream> str = this._Source.Write())
            {
                this._Header.Write(str);
            }
        }

        public Memory Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the amount of items in the map.
        /// </summary>
        public long ItemCount
        {
            get
            {
                return this._Header.Items;
            }
        }

        /// <summary>
        /// Gets the amount of buckets for the map. The amount of items can never exceed this number.
        /// </summary>
        public long BucketCount
        {
            get
            {
                return this._Scheme.Buckets;
            }
        }

        /// <summary>
        /// Initializes an empty hashmap in the given memory. The size of the data should be at or greater than the required
        /// size for the hashmap as computed by the scheme.
        /// </summary>
        public static HashMap<TKey, T> Create(Memory Source, Scheme Scheme)
        {
            // Fill data with an empty map
            Header h = new Header()
            {
                Items = 0,
                FirstFreeBucket = 0,
                FirstFilledBucket = 0,
            };
            using (Disposable<OutStream> os = Source.Write())
            {
                h.Write(os);
                for (long t = 0; t < Scheme.Buckets; t++)
                {
                    Scheme.BucketSerialization.Serialize(new Bucket()
                        {
                            Free = true,
                            Reference = t
                        }, os);
                }
            }

            // Create an interface to the map
            return new HashMap<TKey, T>()
            {
                _Scheme = Scheme,
                _Source = Source,
                _Header = h,
            };
        }

        /// <summary>
        /// Restores a hashmap from data given the memory source and the scheme.
        /// </summary>
        public static HashMap<TKey, T> Restore(Memory Source, Scheme Scheme)
        {
            // Get the header
            Header h;
            using (Disposable<InStream> str = Source.Read())
            {
                h = Header.Read(str);
            }

            // Return the map
            return new HashMap<TKey, T>()
            {
                _Scheme = Scheme,
                _Source = Source,
                _Header = h
            };
        }

        /// <summary>
        /// Scheme information for a hashmap.
        /// </summary>
        public class Scheme
        {
            /// <summary>
            /// The amount of buckets in the hashmap.
            /// </summary>
            public long Buckets;

            /// <summary>
            /// The amount of cellar buckets in the initial buckets of the hashmap.
            /// </summary>
            public long CellarBuckets;

            /// <summary>
            /// Method used to serialize a bucket in the hashmap. This must have a fixed size.
            /// </summary>
            public ISerialization<Bucket> BucketSerialization;

            /// <summary>
            /// Method used to hash and equate keys.
            /// </summary>
            public IHashing<TKey> KeyHashing;

            /// <summary>
            /// Gets the total size of memory needed by the hashmap described with this scheme.
            /// </summary>
            public long RequiredSize
            {
                get
                {
                    return Header.Size + this.BucketSerialization.Size.OrExcept * this.Buckets;
                }
            }
        }

        /// <summary>
        /// Header for a hashmap in memory.
        /// </summary>
        public struct Header
        {
            /// <summary>
            /// If the hashmap is not entirely full, this is an index to a free bucket which is likely the start of a cluster of
            /// free buckets. 
            /// </summary>
            public long FirstFreeBucket;

            /// <summary>
            /// If the hashmap is not entirely empty, this is an index to a filled bucket which is likely the start of a cluster of
            /// filled buckets. 
            /// </summary>
            public long FirstFilledBucket;

            /// <summary>
            /// The amount of items in the hashmap.
            /// </summary>
            public long Items;

            /// <summary>
            /// Writes the header to a stream.
            /// </summary>
            public void Write(OutStream Stream)
            {
                Stream.WriteLong(this.FirstFreeBucket);
                Stream.WriteLong(this.FirstFilledBucket);
                Stream.WriteLong(this.Items);
            }

            /// <summary>
            /// Reads a header from a stream.
            /// </summary>
            public static Header Read(InStream Stream)
            {
                return new Header()
                {
                    FirstFreeBucket = Stream.ReadLong(),
                    FirstFilledBucket = Stream.ReadLong(),
                    Items = Stream.ReadLong()
                };
            }

            /// <summary>
            /// The size of the header in bytes.
            /// </summary>
            public const long Size = 8 * 3;
        }

        /// <summary>
        /// A representation of the contents of a slot in the hash map that can store an item.
        /// </summary>
        public struct Bucket
        {
            /// <summary>
            /// The current key for the bucket.
            /// </summary>
            public TKey Key;

            /// <summary>
            /// The current value for the bucket.
            /// </summary>
            public T Value;

            /// <summary>
            /// A reference to another bucket used if this bucket has a key that was not searched for. If this is
            /// self-referential, there are no more possible places to look for the searched key (although the converse is false).
            /// </summary>
            public long Reference;

            /// <summary>
            /// Indicates wether this bucket is free. A free bucket should be interpreted as not having an item and can be assigned
            /// one if needed. If this is true, the key and the value in this struct are undefined and should be disregarded.
            /// </summary>
            public bool Free;

            /// <summary>
            /// Creates a serialization method for a bucket, given the methods used for serializing keys and values. Both of the provided
            /// serializations should have a fixed size.
            /// </summary>
            public static ISerialization<Bucket> CreateSerialization(ISerialization<TKey> KeySerialization, ISerialization<T> ValueSerialization)
            {
                return new _Serialization(KeySerialization, ValueSerialization);
            }

            private class _Serialization : ISerialization<Bucket>
            {
                public _Serialization(ISerialization<TKey> KeySerialization, ISerialization<T> ValueSerialization)
                {
                    this._KeySerialization = KeySerialization;
                    this._ValueSerialization = ValueSerialization;
                    this._Size = KeySerialization.Size.OrExcept + ValueSerialization.Size.OrExcept + StreamSize.Bool + StreamSize.Long;
                }

                public void Serialize(Bucket Object, OutStream Stream)
                {
                    Stream.WriteBool(Object.Free);
                    Stream.WriteLong(Object.Reference);
                    if (Object.Free)
                    {
                        Stream.Advance(this._Size - StreamSize.Bool - StreamSize.Long);
                    }
                    else
                    {
                        this._KeySerialization.Serialize(Object.Key, Stream);
                        this._ValueSerialization.Serialize(Object.Value, Stream);
                    }
                }

                public Bucket Deserialize(InStream Stream)
                {
                    if (Stream.ReadBool())
                    {
                        Bucket b = new Bucket()
                        {
                            Free = true,
                            Reference = Stream.ReadLong()
                        };
                        Stream.Advance(this._Size - StreamSize.Bool - StreamSize.Long);
                        return b;
                    }
                    else
                    {
                        return new Bucket()
                        {
                            Free = false,
                            Reference = Stream.ReadLong(),
                            Key = this._KeySerialization.Deserialize(Stream),
                            Value = this._ValueSerialization.Deserialize(Stream)
                        };
                    }
                }

                public Maybe<long> Size
                {
                    get
                    {
                        return this._Size;
                    }
                }

                private ISerialization<TKey> _KeySerialization;
                private ISerialization<T> _ValueSerialization;
                private long _Size;
            }
        }

        /// <summary>
        /// An item used for iteration.
        /// </summary>
        public struct Item
        {
            /// <summary>
            /// Gets the key for the item.
            /// </summary>
            public TKey Key
            {
                get
                {
                    return Bucket.Key;
                }
            }

            /// <summary>
            /// Gets the value for the item.
            /// </summary>
            public T Value
            {
                get
                {
                    return Bucket.Value;
                }
            }

            /// <summary>
            /// The index of the bucket this item is in.
            /// </summary>
            public long BucketIndex;

            /// <summary>
            /// The data for the bucket this item is in.
            /// </summary>
            public Bucket Bucket;
        }

        /// <summary>
        /// Gets the items (with associated bucket information) in this hashmap.
        /// </summary>
        public IEnumerable<Item> Items
        {
            get
            {
                long cur = this._Header.FirstFilledBucket;
                long end = cur;
                Disposable<InStream> str = this._Source.Read(this.GetBucketOffset(cur));
                try
                {
                    while (true)
                    {
                        // Get bucket
                        long bi = cur;
                        Bucket b = this._Scheme.BucketSerialization.Deserialize(str);
                        if (!b.Free)
                        {
                            yield return new Item()
                            {
                                BucketIndex = bi,
                                Bucket = b
                            };
                        }

                        // Prepare for reading next bucket
                        cur++;
                        if (cur == this.BucketCount)
                        {
                            cur = 0;
                            str.Dispose();
                            str = this._Source.Read(this.GetBucketOffset(cur));
                        }

                        // Exit if needed
                        if (cur == end)
                        {
                            yield break;
                        }
                    }
                }
                finally
                {
                    str.Dispose();
                }
            }
        }

        private Memory _Source;
        private Header _Header;
        private Scheme _Scheme;
    }

    
}