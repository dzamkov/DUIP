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
    public class World
    {
        public World()
        {
            this._GridSize = new LVector(2, 2);
            this._ClientWorld = new ClientWorld();
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
        /// Gets the clientworld used to represent this world.
        /// </summary>
        public ClientWorld ClientWorld
        {
            get
            {
                return this._ClientWorld;
            }
        }

        private LVector _GridSize;
        private ClientWorld _ClientWorld;
    }

    /// <summary>
    /// Defines how a world should be viewed/represented on a client.
    /// </summary>
    public class ClientWorld
    {
        public ClientWorld()
        {
            this._GridRelationCacheSize = 1;
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

        private int _GridRelationCacheSize;
    }
}