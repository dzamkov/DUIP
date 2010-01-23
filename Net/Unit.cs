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
    /// Representation of a peer, collection of peers or network whose internal networking and logic
    /// function independently from other units.
    /// </summary>
    public class Unit : Resource
    {
        internal Unit(World World) : base(World)
        {

        }

        protected override void SerializeGlobal(BinaryWriteStream Stream)
        {
            if (this._Parent == null)
            {
                ID.Blank().Serialize(Stream);
            }
            else
            {
                this._Parent.ResourceID.Serialize(Stream);
            }
        }

        protected override void DeserializeGlobal(BinaryReadStream Stream, FindResourceHandler FindResource)
        {
            ID parentid = new ID(Stream);
            if (parentid == ID.Blank())
            {
                this._Parent = null;
            }
            else
            {
                this._Parent = (SwarmUnit)FindResource(parentid, this.World);
            }
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

        internal SwarmUnit _Parent;
    }

    /// <summary>
    /// A unit formed by a single peer.
    /// </summary>
    public class PeerUnit : Unit
    {
        internal PeerUnit(World World) : base(World)
        {

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
            this._SubUnits = new List<Unit>();
        }

        /// <summary>
        /// Gets the units that make up this unit.
        /// </summary>
        public IEnumerable<Unit> SubUnits
        {
            get
            {
                return this._SubUnits;
            }
        }

        private List<Unit> _SubUnits;
    }
}
