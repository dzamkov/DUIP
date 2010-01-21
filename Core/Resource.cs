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
    public abstract class Resource
    {
        public Resource(World World, ID ID)
        {
            this._ID = ID;
            this._World = World;
            if (this._World != null)
            {
                this._Add();
            }
        }

        public void Serialize(BinaryWriteStream Target)
        {
            // Either serialize the ID of the resource or its global data instead.
            throw new Exception("Serializing a resource is stupid; care to reconsider?");
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
        /// <param name="Data">The data containing the global state of the
        /// resource.</param>
        protected abstract void OnReceiveData(object Data);

        /// <summary>
        /// Initializes the local state of the resource based on the current global state
        /// it has.
        /// </summary>
        protected virtual void OnLocalInit()
        {
        }

        /// <summary>
        /// Initializes the global state of the resource if no data has been received for it. This
        /// is called only one time in the resources life, when it is created.
        /// </summary>
        protected virtual void OnGlobalInit()
        {
        }

        /// <summary>
        /// Gets the data that stores the global state of this resource.
        /// </summary>
        protected abstract object Data { get; }

        /// <summary>
        /// Gets the world this resource belongs to.
        /// </summary>
        public World World
        {
            get
            {
                return this._World;
            }
            internal set
            {
                // Setting should only be done when the world is previously null.
                this._World = value;
                this._Add();
            }
        }

        /// <summary>
        /// Adds this resource to its world.
        /// </summary>
        private void _Add()
        {
            if(this._World._Resources.ContainsKey(this._ID))
            {
                throw new Exception("Resource with this ID already exists");
            }
            this._World._Resources.Add(this._ID, this);
            this.OnGlobalInit();
            this.OnLocalInit();
        }

        /// <summary>
        /// Gets a resource by an id.
        /// </summary>
        /// <param name="World">The world to search in.</param>
        /// <param name="ID">The id of the resource to find.</param>
        /// <returns>The resource with the id if it exists and has a definit
        /// state; null if no id for that resource is found.</returns>
        public static Resource GetResource(World World, ID ID)
        {
            Resource r;
            if (World._Resources.TryGetValue(ID, out r))
            {
                return r;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a resource descripition that can be used to send or store
        /// the resource. Resource descripitions do not contain data for other
        /// resources used.
        /// </summary>
        /// <param name="Stream">The stream to write the descripition to.</param>
        internal void _CreateResourceDescription(BinaryWriteStream Stream)
        {
            Type type = this.GetType();
            ID typeid = TypeDirectory.GetIDForType(type);

            typeid.Serialize(Stream);
            Core.Serialize.SerializeLong(this.Data, Stream);
        }

        /// <summary>
        /// Loads a resource from a resource description.
        /// </summary>
        /// <param name="World">The world this resource is for.</param>
        /// <param name="ResourceID">The resource id of the resource.</param>
        /// <param name="Stream">The stream to load the resource from.</param>
        /// <returns>The resource loaded from the description.</returns>
        static internal Resource _LoadResourceDescription(World World, ID ResourceID, BinaryReadStream Stream)
        {
            ID typeid = new ID(Stream);
            object data = Core.Serialize.DeserializeLong(Stream);
            Type type = TypeDirectory.GetTypeByID(typeid);

            // Check if world
            if (World == null && type == typeof(World))
            {
                World w = new World(ResourceID);
                w._World = w;
                w._Add();
                w.OnReceiveData(data);
                w.OnLocalInit();
                return w;
            }
            else
            {
                // Create a resource of the type
                ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(World) });
                if (ci != null)
                {
                    Resource r = (Resource)ci.Invoke(new object[] { null });
                    r._ID = ResourceID;
                    r._World = World;
                    r._Add();
                    r.OnReceiveData(data);
                    r.OnLocalInit();
                    return r;
                }
                else
                {
                    throw new Exception("Resource instance cannot be created.");
                }
            }
        }

        private ID _ID;
        private World _World;
    }

    /// <summary>
    /// A function that is part of a resource that is called on a global scale. If any entity on
    /// the network invokes it, it will be called on every instance of that resource. Note that
    /// calling the delegate directly won't accomplish this and it has to be called by the entity.
    /// </summary>
    /// <param name="Params">The parameters for the function.</param>
    /// <param name="Entity">The entity that invoked the function.</param>
    public delegate void GlobalFunction(object Params, Entity Entity);

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
