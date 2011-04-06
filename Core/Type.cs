using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Method of storing a type.
    /// </summary>
    public enum TypeMode : byte
    {
        Reflexive,
        Function,
        Reference
    }

    /// <summary>
    /// A type of data that can be stored on the network. It may be assumed that types will not have their instances
    /// used with other types and networks.
    /// </summary>
    /// <remarks>Types can be viewed as a range of possible values and a 
    /// way to convert them into a data representation.</remarks>
    /// <typeparam name="TInstance">An instance of this type.</typeparam>
    public abstract class Type<TInstance> : Type
    {
        /// <summary>
        /// Serializes an instance of this type to an output stream.
        /// </summary>
        public abstract void Serialize(Context Context, TInstance Instance, OutByteStream Stream);

        /// <summary>
        /// Deserializes an instance of this type from a stream, or returns null if not possible.
        /// </summary>
        public abstract Query<TInstance> Deserialize(Context Context, InByteStream Stream);

        internal sealed override F _Resolve<F>(Type._IResolver<F> Resolver)
        {
            return Resolver.Resolve(this);
        }
    }

    /// <summary>
    /// A generalization of type with no specific instance type.
    /// </summary>
    public abstract class Type
    {
        internal Type()
        {

        }

        /// <summary>
        /// Gets the reflexive type.
        /// </summary>
        public static ReflexiveType Reflexive
        {
            get
            {
                return ReflexiveType.Singleton;
            }
        }

        /// <summary>
        /// Gets a function type.
        /// </summary>
        public static Type Function(Type Argument, Type Result)
        {
            return Argument._Resolve(new _FunctionResolver()
            {
                Result = Result
            });
        }

        private class _FunctionResolver : _IResolver<Type>
        {
            public Type Resolve<T>(Type<T> Type)
            {
                return Result._Resolve(new _ResultResolver<T>()
                {
                    Argument = Type
                });
            }

            private class _ResultResolver<TArg> : _IResolver<Type>
            {
                public Type Resolve<T>(Type<T> Type)
                {
                    return DUIP.Function.Type(this.Argument, Type);
                }

                public Type<TArg> Argument;
            }

            public Type Result;
        }

        /// <summary>
        /// Gets a reference type.
        /// </summary>
        public static Type Reference(bool Force, bool Secured, Type Target)
        {
            return Target._Resolve(new _ReferenceResolver()
            {
                Force = Force,
                Secured = Secured
            });
        }

        private class _ReferenceResolver : _IResolver<Type>
        {
            public Type Resolve<T>(Type<T> Type)
            {
                return DUIP.Reference.Type(this.Force, this.Secured, Type);
            }

            public bool Force;
            public bool Secured;
        }

        /// <summary>
        /// Serializes this type to an output stream.
        /// </summary>
        protected abstract void SerializeType(Context Context, OutByteStream Stream);

        internal void _SerializeType(Context Context, OutByteStream Stream)
        {
            this.SerializeType(Context, Stream);
        }

        internal abstract F _Resolve<F>(_IResolver<F> Resolver);

        internal interface _IResolver<F>
        {
            F Resolve<T>(Type<T> Type);
        }
    }

    /// <summary>
    /// A type whose instances are types.
    /// </summary>
    public class ReflexiveType : Type<Type>
    {
        internal ReflexiveType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static ReflexiveType Singleton = new ReflexiveType();

        public override void Serialize(Context Context, Type Instance, OutByteStream Stream)
        {
            Instance._SerializeType(Context, Stream);
        }

        public override Query<Type> Deserialize(Context Context, InByteStream Stream)
        {
            TypeMode mode = (TypeMode)Stream.Read();
            switch (mode)
            {
                case TypeMode.Reflexive:
                    return this;
                case TypeMode.Function:
                    Type arg = this.Deserialize(Context, Stream);
                    Type res = this.Deserialize(Context, Stream);
                    return Type.Function(arg, res);
                case TypeMode.Reference:
                    bool force = Stream.ReadBool();
                    bool secured = Stream.ReadBool();
                    Type target = this.Deserialize(Context, Stream);
                    return Type.Reference(force, secured, target);
            }
            return null;
        }

        protected override void SerializeType(Context Context, OutByteStream Stream)
        {
            Stream.Write((byte)TypeMode.Reflexive);
        }
    }
}