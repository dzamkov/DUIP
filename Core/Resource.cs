//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DUIP.Core
{
    /// <summary>
    /// An object that is stored across the network.
    /// </summary>
    public abstract class Resource : Serializable
    {
        public Resource(ID ID)
        {
            this._ID = ID;
            if (!_Resources.ContainsKey(ID))
            {
                _Resources.Add(ID, new WeakReference(this));
            }
            else
            {
                throw new Exception("ID already in use");
            }
        }

        public void Serialize(BinaryWriteStream Target)
        {
            this._ID.Serialize(Target);
        }

        public Resource Deserialize(BinaryReadStream Stream)
        {
            ID id = new ID(Stream);
            WeakReference wr;
            if (_Resources.TryGetValue(id, out wr))
            {
                if (wr.IsAlive)
                {
                    return (Resource)wr.Target;
                }
                else
                {
                    _Resources[id] = null;
                }
            }
            return new UnresolvedResource(id);
        }

        static Resource()
        {
            _Resources = new Dictionary<ID, WeakReference>();
        }

        /// <summary>
        /// Gets the id that uniquely identifies this resource across the
        /// network. This should not change throught a resources life.
        /// </summary>
        public ID ResourceID 
        {
            get
            {
                return this._ID;
            }
        }

        /// <summary>
        /// Recreates the global state of the resource based on the data given.
        /// </summary>
        /// <param name="Data">The data containing the global state of </param>
        public abstract void OnReceiveData(Serializable Data);

        /// <summary>
        /// Gets the data that stores the global state of this resource.
        /// </summary>
        public abstract Serializable Data { get; }

        /// <summary>
        /// Gets if this resource is resolved. An unresolved resource can not have functions called,
        /// or store data and is useless until resolved.
        /// </summary>
        public bool Resolved
        {
            get
            {
                return this._Resolved;
            }
        }

        /// <summary>
        /// Gets if this resource is resolved.
        /// </summary>
        internal virtual bool _Resolved
        {
            get
            {
                return true;
            }
        }

        private ID _ID;
        private static Dictionary<ID, WeakReference> _Resources;
    }

    /// <summary>
    /// A resource whose exact type and data is unknown. This type of resource still has
    /// an id.
    /// </summary>
    public sealed class UnresolvedResource : Resource
    {
        internal UnresolvedResource(ID ID) : base(ID)
        {

        }

        public override void OnReceiveData(Serializable Data)
        {
            
        }

        public override string ToString()
        {
            return "<Unresolved Resource>";
        }

        internal override bool _Resolved
        {
            get
            {
                return false;
            }
        }

        public override Serializable Data
        {
            get 
            {
                return null;
            }
        }
    }

    /// <summary>
    /// A function that is part of a resource that is called on a global scale. If any entity on
    /// the network invokes it, it will be called on every instance of that resource. Note that
    /// calling the delegate directly won't accomplish this and it has to be called by the entity.
    /// </summary>
    /// <param name="Params">The parameters for the function.</param>
    /// <param name="Entity">The entity that invoked the function.</param>
    public delegate void GlobalFunction(Serializable Params, Entity Entity);

    /// <summary>
    /// A call on a global function.
    /// </summary>
    internal struct FunctionCall
    {
        /// <summary>
        /// The resource the function is called on.
        /// </summary>
        public Resource Resource;

        /// <summary>
        /// The method in the method info to call.
        /// </summary>
        public string Method;

        /// <summary>
        /// Gets or sets the global function that represents this call.
        /// </summary>
        public GlobalFunction Call
        {
            get
            {
                Type t = this.Resource.GetType();
                Delegate d = Delegate.CreateDelegate(t, this.Resource, this.Method);
                return (GlobalFunction)d;
            }
            set
            {
                this.Method = value.Method.Name;
                this.Resource = (Resource)value.Target;
            }
        }
    }
}
