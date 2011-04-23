using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An implementation of a hashed map contained within mutable data.
    /// </summary>
    public class HashMap<TKey, T> : PartialMap<TKey, T>
    {
        private HashMap()
        {

        }

        public override bool Immutable
        {
            get
            {
                return false;
            }
        }

        public override Maybe<T> Lookup(TKey Key)
        {
            long bi; Bucket b;
            return this.Lookup(this._KeyHashing.Hash(Key), Key, out bi, out b);
        }

        /// <summary>
        /// Looks up an item in the map.
        /// </summary>
        /// <param name="Hash">The hash of the key using the key hashing method for the hashmap.</param>
        /// <param name="BucketIndex">The index of the bucket in which the item was found.</param>
        /// <param name="Bucket">The contents for the bucket in which the item was found.</param>
        public Maybe<T> Lookup(BigInt Hash, TKey Key, out long BucketIndex, out Bucket Bucket)
        {
            BucketIndex = this.GetBucketIndex(Hash);
            while (true)
            {
                Bucket = this.GetBucket(BucketIndex);
                if (!Bucket.Free && this._KeyHashing.Equal(Bucket.Key, Key))
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

        public override bool Modify(TKey Key, Maybe<T> Value)
        {
            long bi; Bucket b;
            return this.Modify(this._KeyHashing.Hash(Key), Key, Value, out bi, out b);
        }

        /// <summary>
        /// Sets, creates, or removes an item with the given key and value.
        /// </summary>
        /// <param name="Hash">The hash of the key using the key hashing method for the hashmap.</param>
        /// <param name="BucketIndex">The index of the bucket in which the item was set. This is undefined if the item was removed.</param>
        /// <param name="Bucket">The contents for the bucket in which the item was set. This is undefined if the item was removed.</param>
        public bool Modify(BigInt Hash, TKey Key, Maybe<T> Value, out long BucketIndex, out Bucket Bucket)
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
                        if (this._KeyHashing.Equal(Bucket.Key, Key))
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
                        if (this._KeyHashing.Equal(Bucket.Key, Key))
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
            Header h = this._Header;
            long hashbuckets = h.Buckets - h.CellarBuckets;
            return (long)((Hash % (ulong)hashbuckets).ToULong() + (ulong)h.CellarBuckets);
        }

        /// <summary>
        /// Gets the bucket whose hash corresponds with the given key.
        /// </summary>
        public long GetBucketIndex(TKey Key)
        {
            return this.GetBucketIndex(this._KeyHashing.Hash(Key));
        }

        /// <summary>
        /// Retrieves the given bucket.
        /// </summary>
        public Bucket GetBucket(long BucketIndex)
        {
            InStream str = this._Data.Read(this.GetBucketOffset(BucketIndex));
            Bucket b = this._BucketSerialization.Deserialize(str);
            str.Finish();
            return b;
        }

        /// <summary>
        /// Sets the given bucket.
        /// </summary>
        public void SetBucket(long BucketIndex, Bucket Value)
        {
            OutStream str = this._Data.Modify(this.GetBucketOffset(BucketIndex));
            this._BucketSerialization.Serialize(Value, str);
            str.Finish();
        }

        /// <summary>
        /// Searches for the first free or filled (depending on the Free parameter) bucket after the given bucket. The search 
        /// will wrap around to the beginning of the hashmap if needed. This method will not exit unless a suitable bucket is found.
        /// </summary>
        public long SearchBucketIndex(long Start, bool Free)
        {
            Start++;
            long tb = this._Header.Buckets;
            long bs = this._BucketSerialization.Size.OrExcept;
            while (true)
            {
                if (Start == tb)
                {
                    Start = 0;
                }
                InStream str = this._Data.Read(this.GetBucketOffset(Start));
                while (Start < tb)
                {
                    Bucket b = this._BucketSerialization.Deserialize(str);
                    if (b.Free == Free)
                    {
                        str.Finish();
                        return Start;
                    }
                    Start++;
                }
                str.Finish();
                Start = 0;
            }
        }

        /// <summary>
        /// Gets the location of the given bucket in the data for the map.
        /// </summary>
        public long GetBucketOffset(long Bucket)
        {
            return Header.Size + this._BucketSerialization.Size.OrExcept * Bucket;
        }

        /// <summary>
        /// Gets if a new item can be added.
        /// </summary>
        private bool _CanAdd
        {
            get
            {
                return this._Header.Items < this._Header.Buckets;
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
            if (this._Header.Items < this._Header.Buckets)
            {
                this._Header.FirstFreeBucket = this.SearchBucketIndex(ind, true);
            }
            return ind;
        }

        /// <summary>
        /// Modifies (but does not update) the header to reflect the given bucket as used.
        /// </summary>
        private void _Add(long Bucket)
        {
            this._Header.Items++;
            if (Bucket == this._Header.FirstFreeBucket && this._Header.Items < this._Header.Buckets)
            {
                this._Header.FirstFreeBucket = this.SearchBucketIndex(Bucket, true);
            }
        }

        /// <summary>
        /// Modifies (but does not update) the header to reflect the given bucket as free.
        /// </summary>
        private void _Remove(long Bucket)
        {
            this._Header.Items--;
        }

        /// <summary>
        /// Updates the header in the data for the hashmap with the header stored in this interface.
        /// </summary>
        private void _UpdateHeader()
        {
            OutStream str = this._Data.Modify();
            this._Header.Write(str);
            str.Finish();
        }

        /// <summary>
        /// Gets the data source for the hashmap.
        /// </summary>
        public Data Data
        {
            get
            {
                return this._Data;
            }
        }

        /// <summary>
        /// Gets the amount of items in the map.
        /// </summary>
        public long Items
        {
            get
            {
                return this._Header.Items;
            }
        }

        /// <summary>
        /// Gets the amount of buckets for the map. The amount of items can never exceed this number.
        /// </summary>
        public long Buckets
        {
            get
            {
                return this._Header.Buckets;
            }
        }

        /// <summary>
        /// Creates an empty hashmap with the given allocator.
        /// </summary>
        /// <param name="Buckets">The maximum amount of items the hash map can store. Proportional to the size of the required data.</param>
        /// <param name="CellarBuckets">The amount of buckets to be used exclusively in the case of hash collisions.</param>
        public static HashMap<TKey, T> Create<TRef>(
            Allocator<TRef> Allocator,
            long Buckets,
            long CellarBuckets,
            ISerialization<Bucket> BucketSerialization,
            IHashing<TKey> KeyHashing,
            out TRef Ref)
        {
            // Allocate required data
            long headersize = Header.Size;
            long bucketsize = BucketSerialization.Size.OrExcept;
            long totalsize = headersize + Buckets * bucketsize;
            Data data; Ref = Allocator.Allocate(totalsize, out data);
            if (data == null)
            {
                return null;
            }

            // Fill data with an empty map
            Header h = new Header()
            {
                Buckets = Buckets,
                CellarBuckets = CellarBuckets,
                Items = 0,
                FirstFreeBucket = 0
            };
            OutStream os = data.Modify();
            h.Write(os);
            for (long t = 0; t < Buckets; t++)
            {
                BucketSerialization.Serialize(new Bucket()
                    {
                        Free = true,
                        Reference = t
                    }, os);
            }
            os.Finish();

            // Create an interface to the map
            HashMap<TKey, T> map = new HashMap<TKey, T>()
            {
                _BucketSerialization = BucketSerialization,
                _Data = data,
                _Header = h,
                _KeyHashing = KeyHashing
            };
            return map;
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
                Stream.WriteLong(this.Buckets);
                Stream.WriteLong(this.CellarBuckets);
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
                    Buckets = Stream.ReadLong(),
                    CellarBuckets = Stream.ReadLong(),
                    FirstFreeBucket = Stream.ReadLong(),
                    FirstFilledBucket = Stream.ReadLong(),
                    Items = Stream.ReadLong()
                };
            }

            /// <summary>
            /// The size in bytes of the header.
            /// </summary>
            public const long Size = 8 * 5;
        }

        /// <summary>
        /// A representation of a mutable area in the hash map that can store an item.
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
                    this._Size = KeySerialization.Size.OrExcept + ValueSerialization.Size.OrExcept + 1 + 8;
                }

                public void Serialize(Bucket Object, OutStream Stream)
                {
                    Stream.WriteBool(Object.Free);
                    Stream.WriteLong(Object.Reference);
                    if (Object.Free)
                    {
                        Stream.Advance(this._Size - 1 - 8);
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
                        Stream.Advance(this._Size - 1 - 8);
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
        /// Gets the method used to serialize a bucket in this map. This serialization
        /// must have a fixed size.
        /// </summary>
        public ISerialization<Bucket> BucketSerialization
        {
            get
            {
                return this._BucketSerialization;
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

        private Data _Data;
        private Header _Header;
        private ISerialization<Bucket> _BucketSerialization;
        private IHashing<TKey> _KeyHashing;
    }

    /// <summary>
    /// A map that uses two hashmaps while transferings items from the Secondary hashmap
    /// to the Primary hashmap. The hashing method used for the two maps should be identical.
    /// </summary>
    public class TransferHashMap<TKey, T> : PartialMap<TKey, T>
    {
        public TransferHashMap(HashMap<TKey, T> Primary, HashMap<TKey, T> Secondary)
        {
            this._Primary = Primary;
            this._Secondary = Secondary;
        }

        public override bool Immutable
        {
            get
            {
                return false;
            }
        }

        public override Maybe<T> Lookup(TKey Key)
        {
            BigInt hash = this._Primary.KeyHashing.Hash(Key);

            long sbi, pbi; HashMap<TKey, T>.Bucket sb, pb;
            Maybe<T> sec = this._Secondary.Lookup(hash, Key, out sbi, out sb);
            Maybe<T> pri = this._Primary.Lookup(hash, Key, out pbi, out pb);

            if (!sec.HasValue)
            {
                return pri;
            }
            if (!pri.HasValue)
            {
                if (this._Primary.Modify(hash, Key, sec, out pbi, out pb))
                {
                    pri = sec;
                    this._Secondary.RemoveBucket(sbi, sb);
                }
            }
            else
            {
                this._Secondary.RemoveBucket(sbi, sb);
            }
            
            return pri;
        }

        public override bool Modify(TKey Key, Maybe<T> Value)
        {
            BigInt hash = this._Primary.KeyHashing.Hash(Key);

            long sbi, pbi; HashMap<TKey, T>.Bucket sb, pb;
            this._Secondary.Modify(hash, Key, Maybe<T>.Nothing, out sbi, out sb);
            return this._Primary.Modify(hash, Key, Value, out pbi, out pb);
        }

        /// <summary>
        /// Gets the primary hashmap. Overtime, items will transfered from the secondary hashmap
        /// to this hashmap. New items will be created with this hashmap.
        /// </summary>
        public HashMap<TKey, T> Primary
        {
            get
            {
                return this._Primary;
            }
        }

        /// <summary>
        /// Gets the secondary hashmap. Overtime, items will be removed from this hashmap and added
        /// to the primary hashmap. No new items will be added to this hashmap unless it is explicitly
        /// used.
        /// </summary>
        public HashMap<TKey, T> Secondary
        {
            get
            {
                return this._Secondary;
            }
        }

        private HashMap<TKey, T> _Primary;
        private HashMap<TKey, T> _Secondary;
    }
}