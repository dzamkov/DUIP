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
        /// Creates a tuple with no parts.
        /// </summary>
        public static Tuple Create()
        {
            return new Tuple(new Part[0]);
        }

        /// <summary>
        /// Creates a tuple with one part. 
        /// </summary>
        /// <remarks>
        /// The parameter types should be given explicitly as type inference may get
        /// a type that is too specialized for the tuple.
        /// </remarks>
        public static Tuple Create<TA>(TA A)
        {
            return new Tuple(new Part[]
            {
                new Part<TA>(A)
            });
        }

        /// <summary>
        /// Creates a tuple with two parts.
        /// </summary>
        /// <remarks>
        /// The parameter types should be given explicitly as type inference may get
        /// a type that is too specialized for the tuple.
        /// </remarks>
        public static Tuple Create<TA, TB>(TA A, TB B)
        {
            return new Tuple(new Part[]
            {
                new Part<TA>(A),
                new Part<TB>(B)
            });
        }

        /// <summary>
        /// Creates a tuple with three parts.
        /// </summary>
        /// <remarks>
        /// The parameter types should be given explicitly as type inference may get
        /// a type that is too specialized for the tuple.
        /// </remarks>
        public static Tuple Create<TA, TB, TC>(TA A, TB B, TC C)
        {
            return new Tuple(new Part[]
            {
                new Part<TA>(A),
                new Part<TB>(B),
                new Part<TC>(C)
            });
        }

        /// <summary>
        /// Creates a tuple with four parts.
        /// </summary>
        /// <remarks>
        /// The parameter types should be given explicitly as type inference may get
        /// a type that is too specialized for the tuple.
        /// </remarks>
        public static Tuple Create<TA, TB, TC, TD>(TA A, TB B, TC C, TD D)
        {
            return new Tuple(new Part[]
            {
                new Part<TA>(A),
                new Part<TB>(B),
                new Part<TC>(C),
                new Part<TD>(D)
            });
        }

        /// <summary>
        /// Creates a tuple with five parts.
        /// </summary>
        /// <remarks>
        /// The parameter types should be given explicitly as type inference may get
        /// a type that is too specialized for the tuple.
        /// </remarks>
        public static Tuple Create<TA, TB, TC, TD, TE>(TA A, TB B, TC C, TD D, TE E)
        {
            return new Tuple(new Part[]
            {
                new Part<TA>(A),
                new Part<TB>(B),
                new Part<TC>(C),
                new Part<TD>(D),
                new Part<TE>(E)
            });
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

        public override Tuple Deserialize(Context Context, InStream Stream)
        {
            Tuple.Part[] parts = new Tuple.Part[this._Parts.Length];
            for (int t = 0; t < this._Parts.Length; t++)
            {
                parts[t] = this._Parts[t].Resolve(new _TypeResolver()
                {
                    Context = Context,
                    Stream = Stream
                });
            }
            return new Tuple(parts);
        }

        private class _TypeResolver : Type.IResolver<Tuple.Part>
        {
            public Tuple.Part Resolve<T>(Type<T> Type)
            {
                return new Tuple.Part<T>(Type.Deserialize(Context, Stream));
            }

            public Context Context;
            public InStream Stream;
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Tuple);
            Stream.WriteInt(this._Parts.Length);
            for (int t = 0; t < this._Parts.Length; t++)
            {
                Type.Reflexive.Serialize(Context, this._Parts[t], Stream);
            }
        }

        private Type[] _Parts;
    }
}