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
        /// <summary>
        /// Creates a blank unfilled general sector.
        /// </summary>
        /// <param name="Size">The size of all units in the grid.</param>
        public GeneralSector(LVector Size)
        {
            this._Size = Size;
            this._Children = new GeneralSector[this._Size.Right, this._Size.Down];
        }

        public override Sector GetChild(LVector Child)
        {
            GeneralSector child = this._Children[Child.Right, Child.Down];
            if (child == null)
            {
                child = this._Children[Child.Right, Child.Down] = new GeneralSector(this._Size);
                child._ChildRelation = Child;
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

        private LVector _Size;
        private GeneralSector[,] _Children;
        private GeneralSector _Parent;
        private LVector _ChildRelation;
    }
}
