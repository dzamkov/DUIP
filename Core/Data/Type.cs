﻿using System;
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
        Reference,
        Void,
        Any,
        Bool
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
        public abstract void Serialize(Context Context, TInstance Instance, OutStream Stream);

        /// <summary>
        /// Deserializes an instance of this type from a stream, or returns null if not possible.
        /// </summary>
        public abstract Query<TInstance> Deserialize(Context Context, InStream Stream);

        public sealed override F Resolve<F>(Type.IResolver<F> Resolver)
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
        /// Gets the reflexive type, a type whose instances are all types including the reflexive type itself.
        /// </summary>
        public static ReflexiveType Reflexive
        {
            get
            {
                return ReflexiveType.Singleton;
            }
        }

        /// <summary>
        /// Gets the void type, a type with only one instance.
        /// </summary>
        public static VoidType Void
        {
            get
            {
                return VoidType.Singleton;
            }
        }

        /// <summary>
        /// Gets the any type, a type whose instances include all instances of all types.
        /// </summary>
        public static AnyType Any
        {
            get
            {
                return AnyType.Singleton;
            }
        }

        /// <summary>
        /// Gets the boolean type, a type whose instances are true and false.
        /// </summary>
        public static BoolType Bool
        {
            get
            {
                return BoolType.Singleton;
            }
        }

        /// <summary>
        /// Gets a function type.
        /// </summary>
        public static Type Function(Type Argument, Type Result)
        {
            return Argument.Resolve(new _FunctionResolver()
            {
                Result = Result
            });
        }

        private class _FunctionResolver : IResolver<Type>
        {
            public Type Resolve<T>(Type<T> Type)
            {
                return Result.Resolve(new _ResultResolver<T>()
                {
                    Argument = Type
                });
            }

            private class _ResultResolver<TArg> : IResolver<Type>
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
        public static Type Reference(bool Force, bool Secured, bool Static, Type Target)
        {
            return Target.Resolve(new _ReferenceResolver()
            {
                Force = Force,
                Secured = Secured,
                Static = Static
            });
        }

        private class _ReferenceResolver : IResolver<Type>
        {
            public Type Resolve<T>(Type<T> Type)
            {
                return DUIP.Reference.Type(this.Force, this.Secured, this.Static, Type);
            }

            public bool Force;
            public bool Secured;
            public bool Static;
        }

        /// <summary>
        /// Serializes this type to an output stream.
        /// </summary>
        protected abstract void SerializeType(Context Context, OutStream Stream);

        internal void _SerializeType(Context Context, OutStream Stream)
        {
            this.SerializeType(Context, Stream);
        }

        public abstract F Resolve<F>(IResolver<F> Resolver);

        public interface IResolver<F>
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

        public override void Serialize(Context Context, Type Instance, OutStream Stream)
        {
            Instance._SerializeType(Context, Stream);
        }

        public override Query<Type> Deserialize(Context Context, InStream Stream)
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
                    bool stat = Stream.ReadBool();
                    Type target = this.Deserialize(Context, Stream);
                    return Type.Reference(force, secured, stat, target);
                case TypeMode.Void:
                    return Type.Void;
                case TypeMode.Any:
                    return Type.Any;
            }
            return null;
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Reflexive);
        }
    }
}