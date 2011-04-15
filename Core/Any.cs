using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An instance of any type.
    /// </summary>
    public class Any<T> : Any
    {
        public Any(Type<T> Type, T Value)
        {
            this._Type = Type;
            this._Value = Value;
        }

        /// <summary>
        /// Gets the type this is an instance of.
        /// </summary>
        public Type<T> Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// The value of this when interpreted with the associated type.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public override Type GeneralType
        {
            get
            {
                return this._Type;
            }
        }

        public override void Serialize(Context Context, OutStream Stream)
        {
            DUIP.Type.Reflexive.Serialize(Context, this._Type, Stream);
            this._Type.Serialize(Context, this._Value, Stream);
        }

        private Type<T> _Type;
        private T _Value;
    }

    /// <summary>
    /// Generalized form of Any.
    /// </summary>
    public abstract class Any
    {
        /// <summary>
        /// Creates a instance of the AnyType.
        /// </summary>
        public static Any<T> Create<T>(Type<T> Type, T Value)
        {
            return new Any<T>(Type, Value);
        }

        /// <summary>
        /// Gets the general form of the type for the instance.
        /// </summary>
        public abstract Type GeneralType { get; }

        /// <summary>
        /// Serializes this object to a stream.
        /// </summary>
        public abstract void Serialize(Context Context, OutStream Stream);
    }

    /// <summary>
    /// A type whose instances include any instance of any type.
    /// </summary>
    public class AnyType : Type<Any>
    {
        private AnyType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly AnyType Singleton = new AnyType();

        public override void Serialize(Context Context, Any Instance, OutStream Stream)
        {
            Instance.Serialize(Context, Stream);
        }

        public override Any Deserialize(Context Context, InStream Stream)
        {
            Type t = Type.Reflexive.Deserialize(Context, Stream);
            return t.Resolve(new _TypeResolver()
            {
                Context = Context,
                Stream = Stream
            });
        }

        private class _TypeResolver : Type.IResolver<Any>
        {
            public Any Resolve<T>(Type<T> Type)
            {
                return new Any<T>(
                    Type,
                    Type.Deserialize(Context, Stream));
            }

            public Context Context;
            public InStream Stream;
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Any);
        }
    }
}