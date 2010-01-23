//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using DUIP.Core;

namespace DUIP.Net
{
    /// <summary>
    /// Representation of a peer, collection of peers or network whose internal networking and logic
    /// function independently from other units.
    /// </summary>
    public class Unit : Resource
    {
        internal Unit(World World) : base(World)
        {

        }

        public Unit(World World, Ptr<SwarmUnit> Parent)
            : base(World)
        {
            this._Parent = Parent;
        }

        protected override void SerializeGlobal(BinaryWriteStream Stream)
        {
            this._Parent.Serialize(Stream);
        }

        protected override void DeserializeGlobal(BinaryReadStream Stream, FindResourceHandler FindResource)
        {
            this._Parent = new Ptr<SwarmUnit>(Stream);
            this._Parent.Resolve(this.World, FindResource);
        }

        /// <summary>
        /// Gets the parent unit of this unit. This unit is a member of its parent and should communicate
        /// with all subunits of the parent.
        /// </summary>
        public SwarmUnit Parent
        {
            get
            {
                return this._Parent;
            }
        }

        internal Ptr<SwarmUnit> _Parent;
    }

    /// <summary>
    /// A unit formed by a single peer.
    /// </summary>
    public class PeerUnit : Unit
    {
        internal PeerUnit(World World) : base(World)
        {

        }

        public PeerUnit(World World, Ptr<SwarmUnit> Parent, Peer Peer)
            : base(World, Parent)
        {
            this._Peer = Peer;
        }

        protected override void SerializeGlobal(BinaryWriteStream Stream)
        {
            IPEndPoint end = this._Peer.Location;
            byte[] addrbyte = end.Address.GetAddressBytes();

            Stream.WriteInt(end.Port);
            Stream.WriteInt(addrbyte.Length);
            Stream.Write(addrbyte);

            base.SerializeGlobal(Stream);
        }

        protected override void DeserializeGlobal(BinaryReadStream Stream, FindResourceHandler FindResource)
        {
            int port = Stream.ReadInt();
            int adrba = Stream.ReadInt();
            byte[] addrbyte = Stream.AssertRead(adrba);
            IPEndPoint end = new IPEndPoint(new IPAddress(addrbyte), port);

            this._Peer = this.World.Net.GetPeer(end);

            base.DeserializeGlobal(Stream, FindResource);
        }

        /// <summary>
        /// Gets the peer that makes up this unit.
        /// </summary>
        public Peer Peer
        {
            get
            {
                return this._Peer;
            }
        }

        private Peer _Peer;
    }

    /// <summary>
    /// A unit formed by a collection of other units.
    /// </summary>
    public class SwarmUnit : Unit
    {
        internal SwarmUnit(World World)
            : base(World)
        {

        }

        public SwarmUnit(World World, Ptr<SwarmUnit> Parent) : base(World, Parent)
        {
            this._SubUnits = new List<Ptr<Unit>>();
        }

        protected override void SerializeGlobal(BinaryWriteStream Stream)
        {
            Stream.WriteInt(this._SubUnits.Count);
            foreach (Ptr<Unit> u in this._SubUnits)
            {
                u.Serialize(Stream);
            }

            base.SerializeGlobal(Stream);
        }

        protected override void DeserializeGlobal(BinaryReadStream Stream, FindResourceHandler FindResource)
        {
            this._SubUnits = new List<Ptr<Unit>>();
            int subamount = Stream.ReadInt();
            for (int t = 0; t < subamount; t++)
            {
                this._SubUnits.Add(new Ptr<Unit>(Stream));
            }

            base.DeserializeGlobal(Stream, FindResource);
        }

        /// <summary>
        /// Gets the units that make up this unit.
        /// </summary>
        public IEnumerable<Ptr<Unit>> SubUnits
        {
            get
            {
                return this._SubUnits;
            }
        }

        private List<Ptr<Unit>> _SubUnits;
    }
}
