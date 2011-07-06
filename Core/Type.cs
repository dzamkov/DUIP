using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Identifies a "type of a type" that allows serialization and equality testing of instances of the kind.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Kind : Attribute
    {
        public Kind(byte ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// The identifier for this kind.
        /// </summary>
        public byte ID;

        static Kind()
        {
            _ForID = new Dictionary<byte, Kind>();
            _ForType = new Dictionary<System.Type, Kind>();
            Func<Type, Type, bool> idkindequals = (x, y) => x == y;
            foreach (var kvp in Reflection.SearchAttributes<Kind>())
            {
                Kind kind = kvp.Value;
                _ForID[kind.ID] = kind;
                _ForType[kvp.Key] = kind;

                
                Maybe<Type> instance = Reflection.Cast<Type>(kvp.Key, "Instance");
                if (instance.HasValue)
                {
                    ConstantSerialization<Type> serialization = new ConstantSerialization<Type>(instance.Value);
                    kind._Equals = idkindequals;
                    kind._Serialization = serialization;
                    continue;
                }

                Maybe<Func<Type, Type, bool>> kindequals = 
                    Reflection.Cast<Func<Type, Type, bool>>(kvp.Key, "KindEquals");
                kind._Equals = kindequals.HasValue ? kindequals.Value : idkindequals;

                Maybe<ISerialization<Type>> kindserialization =
                    Reflection.Cast<ISerialization<Type>>(kvp.Key, "KindSerialization");
                kind._Serialization = kindserialization.Value;
            }
        }

        public static Kind ForID(byte ID)
        {
            return _ForID[ID];
        }

        public static Kind ForType(System.Type Type)
        {
            return _ForType[Type];
        }

        public bool Equals(Type A, Type B)
        {
            return this._Equals(A, B);
        }

        public ISerialization<Type> Serialization
        {
            get
            {
                return this._Serialization;
            }
        }

        private static Dictionary<byte, Kind> _ForID;
        private static Dictionary<System.Type, Kind> _ForType;
        private Func<Type, Type, bool> _Equals;
        private ISerialization<Type> _Serialization;
    }

    /// <summary>
    /// An interpretation of values (called instances) within a certain set.
    /// </summary>
    public abstract class Type : IEquality<object>
    {
        /// <summary>
        /// Gets if the two given instances of this type are equivalent. A computational exception may be thrown
        /// in the case of a computational error.
        /// </summary>
        public abstract bool Equal(object A, object B);

        /// <summary>
        /// Gets the serialization method used for an object of this type.
        /// </summary>
        public abstract ISerialization<object> Serialization { get; }
       
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
            if (A == B)
            {
                return true;
            }

            Kind akind = Kind.ForType(A.GetType());
            Kind bkind = Kind.ForType(B.GetType());
            if (akind == bkind)
            {
                return akind.Equals(A, B);
            }
            else
            {
                return false;
            }

            throw new ComputationalException();
        }

        /// <summary>
        /// Writes a type to a stream.
        /// </summary>
        public static void Write(Type Type, OutStream Stream)
        {
            Kind kind = Kind.ForType(Type.GetType());
            Stream.WriteByte(kind.ID);
            kind.Serialization.Write(Type, Stream);
        }

        /// <summary>
        /// Reads a type from a stream.
        /// </summary>
        public static Type Read(InStream Stream)
        {
            Kind kind = Kind.ForID(Stream.ReadByte());
            return kind.Serialization.Read(Stream);
        }
    }

    /// <summary>
    /// A type whose instances are all types (a type of types).
    /// </summary>
    [Kind(0)]
    public sealed class ReflexiveType : Type, ISerialization<Type>, ISerialization<object>
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

        public override ISerialization<object> Serialization
        {
            get
            {
                return this;
            }
        }

        public new void Write(Type Object, OutStream Stream)
        {
            Write(Object, Stream);
        }

        public new Type Read(InStream Stream)
        {
            return Read(Stream);   
        }

        void ISerialization<object>.Write(object Object, OutStream Stream)
        {
            Type t = Object as Type;
            Type.Write(t, Stream);
        }

        object ISerialization<object>.Read(InStream Stream)
        {
            return Type.Read(Stream);
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