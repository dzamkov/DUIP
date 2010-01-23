//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUIP.Core
{
    /// <summary>
    /// Defines the rules and properties in infinitely large grid of sectors.
    /// </summary>
    public sealed class World : Resource
    {
        public World() : base(null, ID.Random())
        {
            this._Resources = new Dictionary<ID, Resource>();
            this.World = this;

            this._GridSize = new LVector(2, 2);
            this._GridRelationSize = 1;
            this._Root = Sector._CreateRoot(this);

            this._InitLocalData();
        }

        internal World(ID ID)
            : base(null, ID)
        {
            // This constuctor is for resource to initially load a world.
            this._Resources = new Dictionary<ID, Resource>();
        }

        protected override void SerializeGlobal(BinaryWriteStream Stream)
        {
            this._GridSize.Serialize(Stream);
            Stream.WriteInt(this._GridRelationSize);
        }

        protected override void DeserializeGlobal(BinaryReadStream Stream, FindResourceHandler FindResource)
        {
            this._GridSize = new LVector(Stream);
            this._GridRelationSize = Stream.ReadInt();
            this._InitLocalData();
        }

        /// <summary>
        /// Gets the size of a sector in this world.
        /// </summary>
        public LVector SectorSize
        {
            get
            {
                return this._GridSize;
            }
        }

        /// <summary>
        /// The amount in any direction from a sector that relations are stored. Common values
        /// are 0 for no caching or 1 to cache the 8 borders of each sector.
        /// </summary>
        public int RelationSize
        {
            get
            {
                return this._GridRelationSize;
            }
        }

        /// <summary>
        /// Gets the root sector for this world. The root sector acts as a point of reference for others.
        /// </summary>
        public Sector Root
        {
            get
            {
                return this._Root;
            }
        }

        /// <summary>
        /// Initializes local data, the kind that isnt global.
        /// </summary>
        private void _InitLocalData()
        {
            
        }

        private LVector _GridSize;
        private int _GridRelationSize;
        private Sector _Root;
        internal Net.NetManager _NetManager;
        internal Dictionary<ID, Resource> _Resources;
    }
}