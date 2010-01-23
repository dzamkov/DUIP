//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

namespace DUIP.Net
{

    /// <summary>
    /// A request to get a resource.
    /// </summary>
    public abstract class ResourceRequest : Message
    {

        public void Serialize(BinaryWriteStream Stream)
        {
            Core.Serialize.SerializeShort(this, typeof(Message), Stream);
        }

        public void Deserialize(BinaryReadStream Stream)
        {
            Core.Serialize.DeserializeShort(this, typeof(Message), Stream);
        }

        /// <summary>
        /// Gets the resource handler used to find resources. This is used on the sender
        /// when the data arrives.
        /// </summary>
        public virtual FindResourceHandler ResourceHandler
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a resource. This should be called from OnReceive.
        /// </summary>
        /// <param name="Res">The resource to send.</param>
        protected void SendResource(Resource Res)
        {
            ResourceDescriptionResponse rdr = new ResourceDescriptionResponse();
            rdr.Resource = Res;
            rdr.ResourceID = Res.ResourceID;
            rdr.Send(this.NetManager, this, this.From);
        }

        /// <summary>
        /// Called when a resource is received.
        /// </summary>
        /// <param name="Res">The resource that is received.</param>
        public virtual void OnGetResource(Resource Res)
        {

        }

        /// <summary>
        /// Called when resources need to be sent. Called on the receiver.
        /// </summary>
        public virtual void OnSendResource()
        {

        }

        /// <summary>
        /// Called when a resource is received.
        /// </summary>
        /// <param name="Res">The resource that has been received.</param>
        internal void _Received(Resource Res)
        {
            this.OnGetResource(Res);
            if (this.ResourceLoad != null)
            {
                this.ResourceLoad.Invoke(Res);
            }
        }

        protected internal override void OnReceive()
        {
            this.OnSendResource();
            this.Remove();
        }

        public event ResourceLoadHandler ResourceLoad;
    }

    /// <summary>
    /// A request for a single specific resource by ID.
    /// </summary>
    public class SimpleResourceRequest : ResourceRequest
    {
        public override void  OnSendResource()
        {
            // Get the world to see if the resource is availiable.
            World World = this.NetManager.World;
            if (World != null)
            {
                Resource Resource = Resource.GetResource(World, this.ResourceID);
                if (Resource != null)
                {
                    this.SendResource(Resource);
                }
            }
        }

        public ID ResourceID;
    }

    /// <summary>
    /// Callback for when a resource is loaded with a request.
    /// </summary>
    /// <param name="Resource">The resource that was loaded.</param>
    public delegate void ResourceLoadHandler(Resource Resource);

    /// <summary>
    /// A request for the details of the world to be described.
    /// </summary>
    public class WorldRequest : ResourceRequest
    {
        public override void  OnSendResource()
        {
            // Send the world over there
            World World = this.NetManager.World;
            if (World != null)
            {
                this.SendResource(World);
            }
        }

        public override void OnGetResource(Resource Res)
        {
            this.NetManager.World = (World)Res;
        }

    }

    /// <summary>
    /// Sucsessful response to a resource description that gives the details of the resource.
    /// </summary>
    public class ResourceDescriptionResponse : NoRespondMessage
    {

        private void Serialize(BinaryWriteStream Stream)
        {
            this.ResourceID.Serialize(Stream);
        }

        private void Deserialize(BinaryReadStream Stream)
        {
            this.ResourceID = new ID(Stream);
        }

        protected internal override void OnDataWrite(BinaryWriteStream Stream)
        {
            Resource.CreateResourceDescription(Stream);
        }

        protected internal override void OnDataRead(BinaryReadStream Stream)
        {
            ResourceRequest rr = this.Parent as ResourceRequest;
            if (rr != null)
            {
                this.ASync(delegate
                {
                    this.Resource = Resource.LoadResourceDescription(
                        this.NetManager.World,
                        this.ID,
                        Stream, rr.ResourceHandler);
                    rr._Received(this.Resource);
                });
            }
        }

        public ID ResourceID;
        public Resource Resource;
    }
}
