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

            this._GlobalData.GridSize = new LVector(2, 2);
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
            Serialize.SerializeShort(this._GlobalData, typeof(GlobalData), Stream);
        }

        protected override void DeserializeGlobal(BinaryReadStream Stream)
        {
            Serialize.DeserializeShort(this._GlobalData, typeof(GlobalData), Stream);
            this._InitLocalData();
        }

        /// <summary>
        /// Gets the size of a sector in this world.
        /// </summary>
        public LVector SectorSize
        {
            get
            {
                return this._GlobalData.GridSize;
            }
        }

        /// <summary>
        /// The amount in any direction from a sector that relations are cached. Common values
        /// are 0 for no caching or 1 to cache the 8 borders of each sector.
        /// </summary>
        public int RelationCacheSize
        {
            get
            {
                return this._GridRelationCacheSize;
            }
        }

        /// <summary>
        /// Initializes local data, the kind that isnt global.
        /// </summary>
        private void _InitLocalData()
        {
            this._GridRelationCacheSize = 1;
        }

        public struct GlobalData
        {
            public LVector GridSize;
        }

        private GlobalData _GlobalData;
        public int _GridRelationCacheSize;
        internal Net.NetManager _NetManager;
        internal Dictionary<ID, Resource> _Resources;
    }
}