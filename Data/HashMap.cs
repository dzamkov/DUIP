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

        public override Maybe<T> Lookup(TKey Key)
        {
            long ind = this.GetBucket(Key);
            while (true)
            {
                Bucket b = this.Get(ind);
                if (b.Free)
                {
                    return Maybe<T>.Nothing;
                }
                if (this._KeyHashing.Equal(b.Key, Key))
                {
                    return b.Value;
                }
                if (b.Reference == ind)
                {
                    return Maybe<T>.Nothing;
                }
                ind = b.Reference;
            }
        }

        public override bool Set(TKey Key, Maybe<T> Value)
        {
            bool add = Value.HasValue;
            Maybe<long> pind = Maybe<long>.Nothing;
            long ind = this.GetBucket(Key);
            while (true)
            {
                Bucket b = this.Get(ind);
                if (b.Free)
                {
                    if (add)
                    {
                        // Adding an item
                        if (this._Header.Items < this._Header.Buckets)
                        {
                            this.Set(ind, new Bucket()
                            {
                                Free = false,
                                Key = Key,
                                Value = Value.Value,
                                Reference = b.Reference
                            });

                            this._Header.Items++;
                            if (ind == this._Header.FirstFreeBucket && this._Header.Items < this._Header.Buckets)
                            {
                                this._Header.FirstFreeBucket = this.SearchFree(ind);
                            }
                            this._UpdateHeader();
                            return true;
                        }
                        return false;
                    }
                }
                else
                {
                    // Is this the right bucket?
                    if (this._KeyHashing.Equal(b.Key, Key))
                    {
                        if (add)
                        {
                            // Replacing an item
                            this.Set(ind, new Bucket()
                            {
                                Free = false,
                                Key = Key,
                                Value = Value.Value,
                                Reference = b.Reference
                            });
                        }
                        else
                        {
                            // Removing an item
                            this.Set(ind, new Bucket()
                            {
                                Free = true,
                                Reference = b.Reference
                            });

                            // Make sure to close the reference from the previous bucket, if any
                            if (pind.HasValue)
                            {
                                Bucket pb = this.Get(pind.Value);
                                pb.Reference = b.Reference == ind ? pind.Value : b.Reference;
                                this.Set(pind.Value, pb);
                            }

                            this._Header.Items--;
                            this._UpdateHeader();
                        }
                        return true;
                    }
                }

                // Traverse the bucket chain
                if (b.Reference == ind)
                {
                    // Bucket chain ended
                    if (add)
                    {
                        // Add anywhere
                        if (this._Header.Items < this._Header.Buckets)
                        {
                            long nind = this._Header.FirstFreeBucket;
                            this.Set(nind, new Bucket()
                            {
                                Free = false,
                                Key = Key,
                                Value = Value.Value,
                                Reference = nind
                            });

                            // Make sure the previous bucket knows about this
                            b.Reference = nind;
                            this.Set(ind, b);

                            this._Header.Items++;
                            if (this._Header.Items < this._Header.Buckets)
                            {
                                this._Header.FirstFreeBucket = this.SearchFree(nind);
                            }
                            this._UpdateHeader();
                            return true;
                        }
                        return false;
                    }
                    return true;
                }

                pind = ind;
                ind = b.Reference;
            }
        }

        /// <summary>
        /// Gets the bucket that corresponds to the given hash.
        /// </summary>
        public long GetBucket(BigInt Hash)
        {
            Header h = this._Header;
            long hashbuckets = h.Buckets - h.CellarBuckets;
            return (long)((Hash % (ulong)hashbuckets).ToULong() + (ulong)h.CellarBuckets);
        }

        /// <summary>
        /// Gets the bucket whose hash corresponds with the given key.
        /// </summary>
        public long GetBucket(TKey Key)
        {
            return this.GetBucket(this._KeyHashing.Hash(Key));
        }

        /// <summary>
        /// Retrieves the given bucket.
        /// </summary>
        public Bucket Get(long Bucket)
        {
            InStream str = this._Data.Read(this.GetOffset(Bucket));
            Bucket b = this._BucketSerialization.Deserialize(str);
            str.Finish();
            return b;
        }

        /// <summary>
        /// Sets the given bucket.
        /// </summary>
        public void Set(long Bucket, Bucket Value)
        {
            OutStream str = this._Data.Modify(this.GetOffset(Bucket));
            this._BucketSerialization.Serialize(Value, str);
            str.Finish();
        }

        /// <summary>
        /// Searches for the first free bucket after the given bucket. The search will wrap around to the beginning of the hashmap if needed. This
        /// method will not exit unless a free bucket is found.
        /// </summary>
        public long SearchFree(long Start)
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
                InStream str = this._Data.Read(this.GetOffset(Start));
                while (Start < tb)
                {
                    Bucket b = this._BucketSerialization.Deserialize(str);
                    if (b.Free)
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
        public long GetOffset(long Bucket)
        {
            return Header.Size + this._BucketSerialization.Size.OrExcept * Bucket;
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
            /// The index of the first bucket with no contents.
            /// </summary>
            public long FirstFreeBucket;

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
                    Items = Stream.ReadLong()
                };
            }

            /// <summary>
            /// The size in bytes of the header.
            /// </summary>
            public const long Size = 8 * 4;
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
}