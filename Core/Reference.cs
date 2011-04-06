using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A reference to a datum on a network.
    /// </summary>
    /// <typeparam name="T">The type of index, and network, this reference is for.</typeparam>
    public class Reference<T> : Reference
    {
        public Reference(T Index)
        {
            this._Index = Index;
        }

        /// <summary>
        /// Gets the index of this reference.
        /// </summary>
        public T Index
        {
            get
            {
                return this._Index;
            }
        }

        private T _Index;
    }

    /// <summary>
    /// An instance of a non-forced reference type that embeds the target instead of referencing a datum.
    /// </summary>
    public class AntiReference<TTar> : Reference
    {
        public AntiReference(TTar Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// Gets the value of the reference.
        /// </summary>
        public TTar Value
        {
            get
            {
                return this._Value;
            }
        }

        private TTar _Value;
    }

    /// <summary>
    /// A generalized reference with no specified index type. Contains static helper functions for references.
    /// </summary>
    public abstract class Reference
    {
        /// <summary>
        /// Creates a reference type.
        /// </summary>
        public static ReferenceType<TTar> Type<TTar>(bool Force, bool Secured, Type<TTar> Target)
        {
            return new ReferenceType<TTar>(Force, Secured, Target);
        }
    }

    /// <summary>
    /// The type for a reference to content of a certain more specific type.
    /// </summary>
    public class ReferenceType<TTar> : Type<Reference>
    {
        internal ReferenceType(bool Force, bool Secured, Type<TTar> Target)
        {
            this._Force = Force;
            this._Secured = Secured;
            this._Target = Target;
        }

        /// <summary>
        /// Gets wether content of this type has to be referenced, or can it be included normally?
        /// </summary>
        public bool Force
        {
            get
            {
                return this._Force;
            }
        }

        /// <summary>
        /// Gets wether an instance can be to a secured datum (a datum with a Viewer set to anything but the
        /// universal actor). If this is false, than all references that reference secured datums will not be
        /// instances of this type. A reference to an unsecured datum is still an instance of a secured reference
        /// type. Note that secured reference types can not be implicitly converted, because access restrictions have
        /// to be checked first.
        /// </summary>
        public bool Secured
        {
            get
            {
                return this._Secured;
            }
        }

        /// <summary>
        /// The target (referenced) type of this reference type;
        /// </summary>
        public Type<TTar> Target
        {
            get
            {
                return this._Target;
            }
        }

        public override void Serialize(Context Context, Reference Instance, OutByteStream Stream)
        {
            if (this._Force)
            {
                (Context as Network).Serialize(Instance, Stream);
            }
            else
            {
                AntiReference<TTar> ar = Instance as AntiReference<TTar>;
                if (ar != null)
                {
                    Stream.WriteBool(true); // Embedded reference
                    this._Target.Serialize(Context, ar.Value, Stream);
                }
                else
                {
                    Stream.WriteBool(false);
                    (Context as Network).Serialize(Instance, Stream);
                }
            }       
        }

        public override Query<Reference> Deserialize(Context Context, InByteStream Stream)
        {
            throw new NotImplementedException();
        }

        protected override void SerializeType(Context Context, OutByteStream Stream)
        {
            Stream.Write((byte)TypeMode.Reference);
            Stream.WriteBool(this._Force);
            Stream.WriteBool(this._Secured);
            Type.Reflexive.Serialize(Context, this._Target, Stream);
        }

        private bool _Secured;
        private bool _Force;
        private Type<TTar> _Target;
    }
}