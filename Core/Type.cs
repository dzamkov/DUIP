using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interpretation of values (called instances) within a certain set.
    /// </summary>
    public abstract class Type
    {
        /// <summary>
        /// Gets if the two given instances of this type are equivalent.
        /// </summary>
        public abstract bool Equal(object A, object B);

        /// <summary>
        /// Creates a block to display an instance of this type.
        /// </summary>
        public virtual UI.Block CreateBlock(object Instance, UI.Theme Theme)
        {
            throw new NotImplementedException();
        }

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
        /// Gets the type for types (the reflexive type).
        /// </summary>
        public static ReflexiveType Reflexive
        {
            get
            {
                return ReflexiveType.Singleton;
            }
        }

        /// <summary>
        /// Creates a function type for the given argument and result types.
        /// </summary>
        public static FunctionType Function(Type Argument, Type Result)
        {
            return FunctionType.Get(Argument, Result);
        }
    }

    /// <summary>
    /// A type whose instances are all types (a type of types).
    /// </summary>
    public class ReflexiveType : Type
    {
        private ReflexiveType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly ReflexiveType Singleton = new ReflexiveType();

        public override bool Equal(object A, object B)
        {
            return DUIP.Type.Equal(A as Type, B as Type);
        }
    }
}