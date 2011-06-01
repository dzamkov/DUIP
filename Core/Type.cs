using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interpretation of values (called instances) within a certain set.
    /// </summary>
    /// <typeparam name="T">A representation of an instance of the type.</typeparam>
    public abstract class Type<T> : Type
    {
        /// <summary>
        /// Gets if the two representations of values in the type represent the same value.
        /// </summary>
        public abstract bool Equal(T A, T B);
    }

    /// <summary>
    /// A kind of composition type that is entirely defined by named properties (possibly interrelated).
    /// </summary>
    public abstract class Struct<T> : Type<T>
    {

    }

    /// <summary>
    /// The generalized form of a type with no specific instance type. Contains function for creating types.
    /// </summary>
    public abstract class Type
    {
        /// <summary>
        /// Gets if the two given types are equivalent.
        /// </summary>
        public static bool Equal(Type A, Type B)
        {
            return A == B;
        }

        /// <summary>
        /// Gets the type for data.
        /// </summary>
        public static DataType Data
        {
            get
            {
                return DataType.Singleton;
            }
        }

        /// <summary>
        /// Gets the type for strings.
        /// </summary>
        public static StringType String
        {
            get
            {
                return StringType.Singleton;
            }
        }

        /// <summary>
        /// Gets the type for files.
        /// </summary>
        public static FileType File
        {
            get
            {
                return FileType.Singleton;
            }
        }

        /// <summary>
        /// Creates a function type for the given argument and result types.
        /// </summary>
        public static Type Function(Type Argument, Type Result)
        {
            return FunctionType.Get(Argument, Result);
        }
    }
}