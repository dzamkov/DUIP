using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An instance of a tuple type. Contains helper functions for tuples.
    /// </summary>
    public struct Tuple
    {
        public Tuple(Part[] Parts)
        {
            this.Parts = Parts;
        }

        /// <summary>
        /// Creates a tuple type with the specified part types.
        /// </summary>
        public static TupleType Type(Type[] Parts)
        {
            return new TupleType(Parts);
        }

        /// <summary>
        /// An implicitly-typed value for a part in a tuple.
        /// </summary>
        public class Part<T> : Part
        {
            public Part(T Value)
            {
                this.Value = Value;
            }

            public override void Serialize(Context Context, Type Type, OutStream Stream)
            {
                (Type as Type<T>).Serialize(Context, this.Value, Stream);
            }

            /// <summary>
            /// Gets the value of this part.
            /// </summary>
            public T Value;
        }

        /// <summary>
        /// A generalized form of a tuple part.
        /// </summary>
        public abstract class Part
        {
            /// <summary>
            /// Serializes this part to a stream when interpreted with the given type.
            /// </summary>
            public abstract void Serialize(Context Context, Type Type, OutStream Stream);
        }

        /// <summary>
        /// The parts for this tuple.
        /// </summary>
        public Part[] Parts;
    }

    /// <summary>
    /// A type made from an ordered fixed-length composition of heterogeneously-typed data.
    /// </summary>
    public class TupleType : Type<Tuple>
    {
        internal TupleType(Type[] Parts)
        {
            this._Parts = Parts;
        }

        /// <summary>
        /// Gets the types of the parts of this tuple.
        /// </summary>
        public Type[] Parts
        {
            get
            {
                return this._Parts;
            }
        }

        public override void Serialize(Context Context, Tuple Instance, OutStream Stream)
        {
            Tuple.Part[] parts = Instance.Parts;
            for (int t = 0; t < parts.Length; t++)
            {
                parts[t].Serialize(Context, this._Parts[t], Stream);
            }
        }

        public override Query<Tuple> Deserialize(Context Context, InStream Stream)
        {
            Query<Tuple.Part>[] qparts = new Query<Tuple.Part>[this._Parts.Length];
            for (int t = 0; t < this._Parts.Length; t++)
            {
                qparts[t] = this._Parts[t].Resolve(new _TypeResolver()
                {
                    Context = Context,
                    Stream = Stream
                });
            }
            return Query<Tuple>.Create(delegate
            {
                Tuple.Part[] parts = new Tuple.Part[qparts.Length];
                for (int t = 0; t < qparts.Length; t++)
                {
                    parts[t] = qparts[t];
                }
                return new Tuple(parts);
            });
        }

        private class _TypeResolver : Type.IResolver<Query<Tuple.Part>>
        {
            public Query<Tuple.Part> Resolve<T>(Type<T> Type)
            {
                return Type.Deserialize(Context, Stream).Bind<Tuple.Part>(x => new Tuple.Part<T>(x));
            }

            public Context Context;
            public InStream Stream;
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Tuple);
            Stream.WriteInt(this._Parts.Length);
            for(int t = 0; t < this._Parts.Length; t++)
            {
                Type.Reflexive.Serialize(Context, this._Parts[t], Stream);
            }
        }

        private Type[] _Parts;
    }
}