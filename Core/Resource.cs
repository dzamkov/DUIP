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

        public Resource(World World)
            : this(World, ID.Random())
        {

        }

        private void Serialize(BinaryWriteStream Target)
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
        /// Serializes the global state of the resource into a stream. The state is later retreieved with
        /// DeserializeGlobal.
        /// </summary>
        /// <param name="Stream">The stream to save the global state into.</param>
        protected abstract void SerializeGlobal(BinaryWriteStream Stream);

        /// <summary>
        /// Deserializes the global state of the resource into the stream. After this, the resource should
        /// be modified to use the new global state.
        /// </summary>
        /// <param name="Stream">The stream to load the global state from.</param>
        /// <param name="FindResource">The handler used to get additional resources.</param>
        protected abstract void DeserializeGlobal(BinaryReadStream Stream, FindResourceHandler FindResource);

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
            lock (this._World._Resources)
            {
                if (this._World._Resources.ContainsKey(this._ID))
                {
                    throw new Exception("Resource with this ID already exists");
                }
                this._World._Resources.Add(this._ID, this);
            }
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
            lock (World._Resources)
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
        }

        /// <summary>
        /// Creates a resource descripition that can be used to send or store
        /// the resource. Resource descripitions do not contain data for the other
        /// resources used.
        /// </summary>
        /// <param name="Stream">The stream to write the descripition to.</param>
        public void CreateResourceDescription(BinaryWriteStream Stream)
        {
            Type type = this.GetType();
            ID typeid = TypeDirectory.GetIDForType(type);

            typeid.Serialize(Stream);
            this.SerializeGlobal(Stream);
        }

        /// <summary>
        /// Loads a resource from a resource description. This function is blocking depending on the find resource
        /// handler.
        /// </summary>
        /// <param name="World">The world this resource is for.</param>
        /// <param name="ResourceID">The resource id of the resource.</param>
        /// <param name="Stream">The stream to load the resource from.</param>
        /// <param name="FindResource">The handler used to look for additional resources that may
        /// be needed by the resource. Specify null to use the default handler.</param>
        /// <returns>The resource loaded from the description.</returns>
        static public Resource LoadResourceDescription(
            World World, 
            ID ResourceID, 
            BinaryReadStream Stream, 
            FindResourceHandler FindResource)
        {
            ID typeid = new ID(Stream);
            Type type = TypeDirectory.GetTypeByID(typeid);

            FindResourceHandler fdh = FindResource == null ? DefaultResourceHandler : FindResource;

            // Check if world
            if (World == null && type == typeof(World))
            {
                World w = new World(ResourceID);
                w._World = w;
                w._Add();
                w.DeserializeGlobal(Stream, fdh);
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
                    r.DeserializeGlobal(Stream, fdh);
                    r._Add();
                    return r;
                }
                else
                {
                    throw new Exception("Resource instance cannot be created.");
                }
            }
        }

        /// <summary>
        /// Combines two find resource handlers.
        /// </summary>
        /// <param name="Primary">The first handler to check for the resource.</param>
        /// <param name="Secondary">The second handler to check if the first was null.</param>
        /// <returns>The resource if either handler found it or null if neither was able to.</returns>
        public static FindResourceHandler Combine(FindResourceHandler Primary, FindResourceHandler Secondary)
        {
            return delegate(ID ResourceID, World World)
            {
                Resource r = Primary(ResourceID, World);
                r = (r == null) ? Secondary(ResourceID, World) : r;
                return r;
            };
        }

        /// <summary>
        /// Gets the find resource handler that looks up the resource in the world's resource table and returns the
        /// resource if it is found or null if it isn't.
        /// </summary>
        public static FindResourceHandler ResourceTableHandler
        {
            get
            {
                return delegate(ID ResourceID, World World)
                {
                    Resource r;
                    if (World._Resources.TryGetValue(ResourceID, out r))
                    {
                        return r;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
        }

        /// <summary>
        /// Gets a find resource handler that will raise an exception if called. This can be used to insure that either the resource
        /// is loaded completely or not at all.
        /// </summary>
        public static FindResourceHandler ErrorResourceHandler
        {
            get
            {
                return delegate
                {
                    throw new NoResourceException();
                };
            }
        }

        /// <summary>
        /// Gets the default find resource handler used to find resources if no other resource handler is specified.
        /// </summary>
        public static FindResourceHandler DefaultResourceHandler
        {
            get
            {
                return Combine(ResourceTableHandler, ErrorResourceHandler);
            }
        }

        private ID _ID;
        private World _World;
    }

    /// <summary>
    /// Exception raised when no resource is available from a find resource handler.
    /// </summary>
    public class NoResourceException : Exception
    {

    }

    /// <summary>
    /// Handler for finding an additional resource while processing another. This function may be
    /// blocking if it means the resource will be found.
    /// </summary>
    /// <param name="ResourceID">The id of the resource to find.</param>
    /// <param name="World">The world where the resource should be in.</param>
    /// <returns>The resource if it is found or null if it isn't.</returns>
    public delegate Resource FindResourceHandler(ID ResourceID, World World);

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

    /// <summary>
    /// Pointer to a resource across the network. This can point to a resource that
    /// is not loaded on the current computer but exists in the network.
    /// </summary>
    /// <typeparam name="R">The type of resource to be pointed to.</typeparam>
    public struct Ptr<R> where R : Resource
    {
        public Ptr(R Res)
        {
            this._Res = Res;
            this._ResID = this._Res.ResourceID;
        }

        public Ptr(BinaryReadStream Stream)
        {
            this._ResID = new ID(Stream);
            this._Res = null;
        }

        public void Serialize(BinaryWriteStream Stream)
        {
            this._ResID.Serialize(Stream);
        }

        /// <summary>
        /// Gets if the resource pointer is null. This does not have to do with
        /// if the current instance has the resource loaded, but instead if the
        /// pointer is null.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this._ResID == ID.Blank();
            }
        }

        /// <summary>
        /// Gets if the resource is loaded on the current instance.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return this._Res != null;
            }
        }

        /// <summary>
        /// Gets the actual resource pointed to by this pointer. This is null if the
        /// pointer is null or if the instance hasn't loaded the resource.
        /// </summary>
        public R Dereference
        {
            get
            {
                return this._Res;
            }
        }

        /// <summary>
        /// Tries loading the resource for this pointer. This call may be blocking.
        /// </summary>
        /// <param name="World">The world to load the resource in.</param>
        /// <returns>The resource when found.</returns>
        public R Resolve(World World)
        {
            return this.Resolve(World, Resource.DefaultResourceHandler);
        }

        /// <summary>
        /// Tries to load the resource for the pointer with the specified handler.
        /// </summary>
        /// <param name="World">The world to load the resource in.</param>
        /// <param name="Handler">The handler to use to find the resource.</param>
        /// <returns>The resource when found.</returns>
        public R Resolve(World World, FindResourceHandler Handler)
        {
            Resource b = Handler(this._ResID, World);
            R a = b as R;
            if (a == null)
            {
                throw new Exception("Resource can not be converted to the specified type.");
            }
            return a;
        }

        public static implicit operator R(Ptr<R> Ptr)
        {
            return Ptr.Dereference;
        }

        private ID _ResID;
        private R _Res;
    }
}
