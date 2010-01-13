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
        public GeneralSector()
        {
            this._ChildSector = 0;
        }

        public override Sector[] Children
        {
            get
            {
                if (this._Children == null)
                {
                    this._Children = new GeneralSector[4];
                    for (int t = 0; t < 4; t++)
                    {
                        this._Children[t] = new GeneralSector();
                        this._Children[t]._ChildSector = t;
                        this._Children[t]._Parent = this;
                    }
                }
                return this._Children;
            }
        }

        public override Sector Parent
        {
            get
            {
                if (this._Parent == null)
                {
                    this._Parent = new GeneralSector();
                    this._Parent._ChildSector = this._ChildSector + 1;
                }
                return this._Parent;
            }
        }

        public override Sector[] Borders
        {
            get
            {
                if (this._BorderCache == null)
                {
                    this._BorderCache = (GeneralSector[])base.Borders;
                }
                return this._BorderCache;
            }
        }

        public override int ChildSector
        {
            get
            {
                return this._ChildSector;
            }
        }

        private GeneralSector[] _Children;
        private GeneralSector[] _BorderCache;
        private GeneralSector _Parent;
        private int _ChildSector;
    }
}
