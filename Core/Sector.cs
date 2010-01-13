using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUIP.Core
{
    /// <summary>
    /// Represents a location within a world in terms of sectors.
    /// </summary>
    public struct Location
    {
        /// <summary>
        /// The sector this location is in relation to.
        /// </summary>
        public Sector Sector;

        /// <summary>
        /// X offset from the top-left corner of the sector from
        /// 0.0 to 1.0.
        /// </summary>
        public double OffsetX;

        /// <summary>
        /// Y offset from the top-left corner of the sector from
        /// 0.0 to 1.0.
        /// </summary>
        public double OffsetY;

        /// <summary>
        /// Brings the location up to the parent sector without changing
        /// the point.
        /// </summary>
        public void UpSector()
        {
            int sc = this.Sector.ChildSector;
            this.OffsetX /= 2.0;
            this.OffsetY /= 2.0;
            if (sc % 2 >= 1) this.OffsetX += 0.5;
            if (sc % 4 >= 2) this.OffsetY += 0.5;
            this.Sector = this.Sector.Parent;
        }

        /// <summary>
        /// Brings the location down to a child sector without changing
        /// the point.
        /// </summary>
        public void DownSector()
        {
            int sc = 0;
            if (this.OffsetX >= 0.5) { sc += 1; this.OffsetX -= 0.5; }
            if (this.OffsetY >= 0.5) { sc += 2; this.OffsetY -= 0.5; }
            this.Sector = this.Sector.Children[sc];
        }

        /// <summary>
        /// Brings to the correct same-level sector that can contain this
        /// location.
        /// </summary>
        /// <returns>If the parent sector was changed.</returns>
        public bool Normalize()
        {
            Sector[] borders = this.Sector.Borders;
            if (this.OffsetY <= 0.0)
            {
                this.OffsetY += 1.0;
                this.Sector = borders[0];
                return true;
            }
            if (this.OffsetX > 1.0)
            {
                this.OffsetX -= 1.0;
                this.Sector = borders[1];
                return true;
            }
            if (this.OffsetY > 1.0)
            {
                this.OffsetY -= 1.0;
                this.Sector = borders[2];
                return true;
            }
            if (this.OffsetX <= 0.0)
            {
                this.OffsetX += 1.0;
                this.Sector = borders[3];
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A square block of spsace that can be subdivided into more sectors.
    /// </summary>
    public abstract class Sector
    {
        /// <summary>
        /// Gets the set of child sectors for this sector. Child sectors have
        /// half the width and length of their parents and are arranged with
        /// child 0 in the top left corner, child 1 in the top right, child
        /// 2 in the bottom left and child 3 in the bottom right.
        /// </summary>
        public abstract Sector[] Children { get; }

        /// <summary>
        /// Gets the parent sector that has this as a child.
        /// </summary>
        public abstract Sector Parent { get; }

        /// <summary>
        /// Gets the index where this sector can be found in its parent such that
        /// Parent.Children[ChildSector] == this.
        /// </summary>
        public virtual int ChildSector
        {
            get
            {
                Sector[] children = this.Parent.Children;
                for (int t = 0; t < 4; t++)
                {
                    if (children[t] == this)
                    {
                        return t;
                    }
                }
                throw new Exception("Shouldn't Happen");
            }
        }

        /// <summary>
        /// Gets the borders for this sector. The borders are arranged clockwise with
        /// border 0 at the top.
        /// </summary>
        public virtual Sector[] Borders
        {
            get
            {
                Sector par = this.Parent;
                Sector[] bords = new Sector[4];
                int sc = this.ChildSector;
                for (int t = 0; t < 4; t++)
                {
                    int m = ((t % 2) * 2) + 2;
                    if ((sc % m >= (m / 2)) ^ (t == 1 || t == 2))
                    {
                        Sector ot = par.Borders[t];
                        bords[t] = ot.Children[(sc + 2) % 4];
                    }
                    else
                    {
                        bords[t] = par.Children[(sc + 1) % 4];
                    }
                }
                return bords;
            }
        }

        /// <summary>
        /// Gets the location at the center of this sector.
        /// </summary>
        public Location Center
        {
            get
            {
                return new Location() { Sector = this, OffsetX = 0.5, OffsetY = 0.5 };
            }
        }
    }
}
