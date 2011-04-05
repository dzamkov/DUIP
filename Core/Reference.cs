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
    /// The type for a reference to content of a certain more specific type.
    /// </summary>
    public class ReferenceType : Type
    {
        public ReferenceType(bool ForceReference, bool Secured, Type Target)
        {
            this._ForceReference = ForceReference;
            this._Secured = Secured;
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
        public Type Target
        {
            get
            {
                return this._Target;
            }
        }

        private bool _Secured;
        private bool _ForceReference;
        private Type _Target;
    }
}