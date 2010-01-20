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
    public class World : Serializable
    {
        public World()
        {
            this._GridSize = new LVector(2, 2);
            this._GridRelationCacheSize = 1;
        }

        public World(BinaryReadStream Stream)
        {
            this._GridSize = new LVector(Stream);
            this._GridRelationCacheSize = Stream.ReadInt();
        }

        public void Serialize(BinaryWriteStream Stream)
        {
            this._GridSize.Serialize(Stream);
            Stream.WriteInt(this._GridRelationCacheSize);
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

        private LVector _GridSize;
        private int _GridRelationCacheSize;
    }
}