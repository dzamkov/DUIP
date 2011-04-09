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
    /// A generalized reference with no specified index type. Contains static helper functions for references.
    /// </summary>
    public abstract class Reference
    {
        /// <summary>
        /// Gets the type for references.
        /// </summary>
        public static ReferenceType Type
        {
            get
            {
                return ReferenceType.Singleton;
            }
        }
    }

    /// <summary>
    /// The type for a reference to a datum on a network.
    /// </summary>
    public class ReferenceType : Type<Reference>
    {
        private ReferenceType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static ReferenceType Singleton = new ReferenceType();

        public override void Serialize(Context Context, Reference Instance, OutStream Stream)
        {
            (Context as Network).Serialize(Instance, Stream);
        }

        public override Query<Reference> Deserialize(Context Context, InStream Stream)
        {
            return (Context as Network).Deserialize(Stream);
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Reference);
        }
    }
}