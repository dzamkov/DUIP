using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A reference to a datum on a network.
    /// </summary>
    /// <typeparam name="T">The type of index, and network, this reference is for.</typeparam>
    public class Reference<T> : Content
    {
        public Reference(T Index, Network<T> Network)
        {
            this._Index = Index;
            this._Network = Network;
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

        /// <summary>
        /// Gets the network this reference is for.
        /// </summary>
        public Network<T> Network
        {
            get
            {
                return this._Network;
            }
        }

        private T _Index;
        private Network<T> _Network;
    }

    /// <summary>
    /// The type for a reference to content of a certain more specific type.
    /// </summary>
    public class ReferenceType : Type
    {
        public ReferenceType(bool ForceReference, Type Target)
        {
            this._ForceReference = ForceReference;
            this._Target = Target;
        }

        /// <summary>
        /// Gets wether content of this type has to be referenced, or can it be included normally?
        /// </summary>
        public bool ForceReference
        {
            get
            {
                return this._ForceReference;
            }
        }

        /// <summary>
        /// The target (referenced) type of this reference type;
        /// </summary>
        public Type Target
        {
            get
            {
                return this._Target;
            }
        }

        private bool _ForceReference;
        private Type _Target;
    }
}