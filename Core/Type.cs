using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A "type of a type" that allows serialization and equality testing of instances of the kind.
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
            Func<Type, Type, bool> idtypeequals = (x, y) => x == y;
            foreach (var kvp in Reflection.SearchAttributes<Kind>())
            {
                Kind kind = kvp.Value;
                _ForID[kind.ID] = kind;
                _ForType[kvp.Key] = kind;

                
                Maybe<Type> instance = Reflection.Cast<Type>(kvp.Key, "Instance");
                if (instance.HasValue)
                {
                    ConstantSerialization<Type> serialization = new ConstantSerialization<Type>(instance.Value);
                    kind._TypeEquals = idtypeequals;
                    kind._GetTypeSerialization = (x) => serialization;
                    continue;
                }

                Maybe<Func<Type, Type, bool>> typeequals = 
                    Reflection.Cast<Func<Type, Type, bool>>(kvp.Key, "TypeEquals");
                kind._TypeEquals = typeequals.HasValue ? typeequals.Value : idtypeequals;

                Maybe<Func<Context, ISerialization<Type>>> gettypeserialization =
                    Reflection.Cast<Func<Context, ISerialization<Type>>>(kvp.Key, "GetTypeSerialization");
                kind._GetTypeSerialization = gettypeserialization.Value;
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

        public bool TypeEquals(Type A, Type B)
        {
            return this._TypeEquals(A, B);
        }

        public ISerialization<Type> GetTypeSerialization(Context Context)
        {
            return this._GetTypeSerialization(Context);
        }

        private static Dictionary<byte, Kind> _ForID;
        private static Dictionary<System.Type, Kind> _ForType;
        private Func<Type, Type, bool> _TypeEquals;
        private Func<Context, ISerialization<Type>> _GetTypeSerialization;
    }

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
            if (A == B)
            {
                return true;
            }

            Kind akind = Kind.ForType(A.GetType());
            Kind bkind = Kind.ForType(B.GetType());
            if (akind == bkind)
            {
                return akind.TypeEquals(A, B);
            }
            else
            {
                return false;
            }

            throw new ComputationalException();
        }
    }

    /// <summary>
    /// A type whose instances are all types (a type of types).
    /// </summary>
    [Kind(0)]
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
            return new TypeSerialization(Context);    
        }
    }

    /// <summary>
    /// A serialization method for types.
    /// </summary>
    public class TypeSerialization : ISerialization<Type>, ISerialization<object>
    {
        public TypeSerialization(Context Context)
        {
            this._Context = Context;
        }

        /// <summary>
        /// Gets the context for this serialization method.
        /// </summary>
        public Context Context
        {
            get
            {
                return this._Context;
            }
        }

        public void Serialize(Type Object, OutStream Stream)
        {
            Kind kind = Kind.ForType(Object.GetType());
            Stream.WriteByte(kind.ID);
            kind.GetTypeSerialization(this._Context).Serialize(Object, Stream);
        }

        public Type Deserialize(InStream Stream)
        {
            byte kindid = Stream.ReadByte();
            Kind kind = Kind.ForID(kindid);
            return kind.GetTypeSerialization(this._Context).Deserialize(Stream);
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

        private Context _Context;
    }
}