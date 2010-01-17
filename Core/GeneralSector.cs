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
        
        private GeneralSector(LVector Size)
        {
            this._Size = Size;
            this._Children = new GeneralSector[this._Size.Right, this._Size.Down];
        }

        /// <summary>
        /// Creates a blank unfilled general sector.
        /// </summary>
        /// <param name="Size">The size of all units in the grid.</param>
        /// <returns>A new general sector.</returns>
        public static GeneralSector Create(LVector Size)
        {
            GeneralSector gs = new GeneralSector(Size);
            gs._Init();
            return gs;
        }

        public override Sector GetChild(LVector Child)
        {
            GeneralSector child = this._Children[Child.Right, Child.Down];
            if (child == null)
            {
                child = this._Children[Child.Right, Child.Down] = new GeneralSector(this._Size);
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
                this._Parent = new GeneralSector(this._Size);
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
                return this._Size;
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
                for (int x = 0; x < this._Size.Right; x++)
                {
                    for (int y = 0; y < this._Size.Down; y++)
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

        /// <summary>
        /// Initializes the sector after its relations are set up.
        /// </summary>
        private void _Init()
        {
            this._VisData = new Visual.SectorVisData(this);
        }

        private LVector _Size;
        private GeneralSector[,] _Children;
        private GeneralSector _Parent;
        private LVector _ChildRelation;
    }
}
