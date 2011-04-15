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

        private Type[] _Parts;
    }
}