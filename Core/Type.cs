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
        /// Gets if the two given instances of this type are equivalent. A computational exception may be thrown
        /// in the case of a computational error.
        /// </summary>
        public abstract bool Equal(object A, object B);

        /// <summary>
        /// Gets the serialization method (with the given serialization context) to use for an object of this type.
        /// </summary>
        public virtual ISerialization<object> GetSerialization(Context Context)
        {
            throw new NotImplementedException();
        }

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
            // Reference comparison
            if (A == B)
            {
                return true;
            }

            // Compare as functions
            FunctionType aft = A as FunctionType;
            if (aft != null)
            {
                FunctionType bft = B as FunctionType;
                if (bft != null)
                {
                    return Equal(aft.Argument, bft.Argument) && Equal(aft.Result, bft.Result);
                }
                return false;
            }
            if (B is FunctionType)
            {
                return false;
            }

            
            throw new ComputationalException();
        }

        /// <summary>
        /// Gets the type for data.
        /// </summary>
        public static DataType Data
        {
            get
            {
                return DataType.Instance;
            }
        }

        /// <summary>
        /// Gets the type for strings.
        /// </summary>
        public static StringType String
        {
            get
            {
                return StringType.Instance;
            }
        }

        /// <summary>
        /// Gets the type for files.
        /// </summary>
        public static FileType File
        {
            get
            {
                return FileType.Instance;
            }
        }

        /// <summary>
        /// Gets the type for types (the reflexive type).
        /// </summary>
        public static ReflexiveType Reflexive
        {
            get
            {
                return ReflexiveType.Instance;
            }
        }

        /// <summary>
        /// Creates a function type for the given argument and result types.
        /// </summary>
        public static FunctionType Function(Type Argument, Type Result)
        {
            return new FunctionType(Argument, Result);
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
        public static readonly ReflexiveType Instance = new ReflexiveType();

        public override bool Equal(object A, object B)
        {
            return DUIP.Type.Equal(A as Type, B as Type);
        }

        public override ISerialization<object> GetSerialization(Context Context)
        {
            return new TypeSerialization();    
        }
    }

    /// <summary>
    /// A serialization method for types.
    /// </summary>
    public class TypeSerialization : ISerialization<Type>, ISerialization<object>
    {
        /// <summary>
        /// Specifies a possible method to use for serialization.
        /// </summary>
        public enum Method : byte
        {
            Reflexive,
            Function,
            String,
            Data,
            File,
        }

        public void Serialize(Type Object, OutStream Stream)
        {
            if (Object is ReflexiveType)
            {
                Stream.WriteByte((byte)Method.Reflexive);
                return;
            }

            FunctionType ft = Object as FunctionType;
            if (ft != null)
            {
                Stream.WriteByte((byte)Method.Function);
                this.Serialize(ft.Argument, Stream);
                this.Serialize(ft.Result, Stream);
                return;
            }

            if (Object is StringType)
            {
                Stream.WriteByte((byte)Method.String);
                return;
            }

            if (Object is DataType)
            {
                Stream.WriteByte((byte)Method.Data);
                return;
            }

            if (Object is FileType)
            {
                Stream.WriteByte((byte)Method.File);
                return;
            }
        }

        public Type Deserialize(InStream Stream)
        {
            switch ((Method)Stream.ReadByte())
            {
                case Method.Reflexive:
                    return Type.Reflexive;
                case Method.Function:
                    Type arg = this.Deserialize(Stream);
                    Type res = this.Deserialize(Stream);
                    return Type.Function(arg, res);
                case Method.String:
                    return Type.String;
                case Method.Data:
                    return Type.Data;
                case Method.File:
                    return Type.File;
                default:
                    throw new DeserializationException();
            }
        }

        void ISerialization<object>.Serialize(object Object, OutStream Stream)
        {
            this.Serialize(Object as Type, Stream);
        }

        object ISerialization<object>.Deserialize(InStream Stream)
        {
            return this.Deserialize(Stream) as Type;
        }

        public Maybe<long> Size
        {
            get
            {
                return Maybe<long>.Nothing;
            }
        }
    }
}