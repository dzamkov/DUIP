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
    /// Implementation of a sector capable of taking over all needed tasks. All relationships with all
    /// other sectors are never going to be null because GeneralSector creates more sectors when requested.
    /// </summary>
    public class GeneralSector : Sector
    {
        
        private GeneralSector(World World)
        {
            this._World = World;
            int rc = this._World.RelationCacheSize;
            this._Children = new GeneralSector[this.Size.Right, this.Size.Down];
            this._RelationCache = new GeneralSector[rc * 2 + 1, rc * 2 + 1];
        }

        /// <summary>
        /// Creates a blank unfilled general sector.
        /// </summary>
        /// <param name="World">The world this unit is for.</param>
        /// <returns>A new general sector.</returns>
        public static GeneralSector Create(World World)
        {
            GeneralSector gs = new GeneralSector(World);
            gs._Init();
            return gs;
        }

        public override Sector GetChild(LVector Child)
        {
            GeneralSector child = this._Children[Child.Right, Child.Down];
            if (child == null)
            {
                child = this._Children[Child.Right, Child.Down] = new GeneralSector(this._World);
                child._Parent = this;
                child._ChildRelation = Child;
                child._Init();
            }
            return child;
        }

        public override Sector GetParent(LVector ChildRelation)
        {
            if (this._Parent == null)
            {
                this._Parent = new GeneralSector(this._World);
                this._ChildRelation = ChildRelation;
                this._Parent._Children[ChildRelation.Right, ChildRelation.Down] = this;
                this._Parent._Init();
            }
            return this._Parent;
        }

        public override LVector ChildRelation
        {
            get
            {
                if (this._Parent == null)
                {
                    this.GetParent(new LVector());
                }
                return this._ChildRelation;
            }
        }

        public override LVector Size
        {
            get
            {
                return this._World.SectorSize;
            }
        }

        public override bool BlankParent
        {
            get
            {
                return this._Parent == null;
            }
        }

        public override IEnumerable<Sector> Children
        {
            get
            {
                for (int x = 0; x < this.Size.Right; x++)
                {
                    for (int y = 0; y < this.Size.Down; y++)
                    {
                        GeneralSector gs = this._Children[x, y];
                        if (gs != null)
                        {
                            yield return gs;
                        }
                    }
                }
            }
        }

        public override Sector GetRelation(LVector Vector)
        {
            int rc = this._World.RelationCacheSize;
            if (Vector.Down >= -rc && Vector.Down <= rc &&
                Vector.Right >= -rc && Vector.Right <= rc)
            {
                LVector diff = Vector + new LVector(rc, rc);
                if (this._RelationCache[diff.Right, diff.Down] != null)
                {
                    return this._RelationCache[diff.Right, diff.Down];
                }
                else
                {
                    return this._RelationCache[diff.Right, diff.Down] = (GeneralSector)base.GetRelation(Vector);
                }
            }
            return base.GetRelation(Vector);
        }

        /// <summary>
        /// Initializes the sector after its relations are set up.
        /// </summary>
        private void _Init()
        {
            this._VisData = new Visual.SectorVisData(this);
        }

        private World _World;
        private GeneralSector[,] _Children;
        private GeneralSector[,] _RelationCache;
        private GeneralSector _Parent;
        private LVector _ChildRelation;
    }
}
