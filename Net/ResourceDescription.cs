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
    /// A request to have the details of a resource described.
    /// </summary>
    public class ResourceDescriptionRequest : Message
    {
        protected internal override void OnReceive()
        {
            // Get the world to see if the resource is availiable.
            World World = this.NetManager.World;
            if (World != null)
            {
                Resource Resource = Resource.GetResource(World, this.ResourceID);
                if (Resource != null)
                {
                    // Resource is available.
                    new ResourceDescriptionResponse { Resource = Resource, ResourceID = Resource.ResourceID }.Send(this.NetManager, this, this.From);
                }
            }
            this.Remove();
        }

        public ID ResourceID;
    }

    /// <summary>
    /// A request for the details of the world to be described.
    /// </summary>
    public class WorldDescriptionRequest : Message
    {
        protected internal override void OnReceive()
        {
            // Send the world over there
            World World = this.NetManager.World;
            if (World != null)
            {
                new ResourceDescriptionResponse { Resource = World, ResourceID = World.ResourceID }.Send(this.NetManager, this, this.From);
            }
            this.Remove();
        }

        protected internal override void OnRespond(Message Response)
        {
            ResourceDescriptionResponse rdr = Response as ResourceDescriptionResponse;
            this.NetManager.World = (World)(rdr.Resource);
            this.Remove();
        }

    }

    /// <summary>
    /// Sucsessful response to a resource description that gives the details of the resource.
    /// </summary>
    public class ResourceDescriptionResponse : Message
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
            Resource._CreateResourceDescription(Stream);
        }

        protected internal override void OnDataRead(BinaryReadStream Stream)
        {
            ResourceDescriptionRequest rdr = this.Parent as ResourceDescriptionRequest;
            WorldDescriptionRequest wdr = this.Parent as WorldDescriptionRequest;
            if (wdr != null || (rdr != null && rdr.ID == this.ID))
            {
                this.Resource = Resource._LoadResourceDescription(
                    this.NetManager.World,
                    this.ID,
                    Stream);
            }
        }

        protected internal override void OnReceive()
        {
            this.Remove();
        }

        protected internal override void OnSend()
        {
            this.Remove();
        }

        public ID ResourceID;
        public Resource Resource;
    }
}
