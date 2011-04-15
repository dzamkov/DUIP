using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type of data that can be stored on the network. It may be assumed that types will not have their instances
    /// used with other types and networks.
    /// </summary>
    /// <remarks>Types can be viewed as a range of possible values and a 
    /// way to convert them into a data representation.</remarks>
    /// <typeparam name="T">An instance of this type.</typeparam>
    public abstract class Type<T> : Type
    {
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
        /// Creates a tuple type with the given types for the parts.
        /// </summary>
        public static TupleType Tuple(Type[] Parts)
        {
            return DUIP.Tuple.Type(Parts);
        }

        /// <summary>
        /// Gets a type for an identifier.
        /// </summary>
        public static IDType ID
        {
            get
            {
                return IDType.Singleton;
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
    }
}