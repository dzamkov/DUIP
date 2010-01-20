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
    /// Represents a location within a world in terms of sectors.
    /// </summary>
    public struct Location
    {
        /// <summary>
        /// The sector this location is in relation to.
        /// </summary>
        public Sector Sector;

        /// <summary>
        /// Offset from the sector.
        /// </summary>
        public SVector Offset;

        /// <summary>
        /// Brings the sector down to one of its children without changing the point.
        /// </summary>
        public void Down()
        {
            LVector size = this.Sector.Size;
            this.Offset.Down *= (double)size.Down;
            this.Offset.Right *= (double)size.Right;
            LVector child = this.Offset.ToLVector();
            this.Sector = this.Sector.GetChild(child);
            this.Offset -= child;
        }

        /// <summary>
        /// Brings the sector up to its parent without changing the point.
        /// </summary>
        public void Up()
        {
            LVector size = this.Sector.Size;
            LVector child = this.Sector.ChildRelation;
            this.Offset += child;
            this.Sector = this.Sector.Parent;
            this.Offset.Down /= (double)size.Down;
            this.Offset.Right /= (double)size.Right;
        }

        /// <summary>
        /// Brings to the correct same-level sector that can contain this
        /// location.
        /// </summary>
        public void Normalize()
        {
            LVector m = this.Offset.ToLVector();
            if (m.Down != 0 || m.Right != 0)
            {
                SVector r = this.Offset - m;
                this.Sector = this.Sector.GetRelation(m);
                this.Offset = r;
            }
        }
    }

    /// <summary>
    /// A square block of space that can be subdivided into more sectors.
    /// </summary>
    public abstract class Sector
    {

        /// <summary>
        /// Gets a child sector of this sector.
        /// </summary>
        /// <param name="Child">The vector that specifies the child to
        /// get. This vector is in relation to the child at the top-left corner
        /// of this sector.</param>
        /// <returns>The specified child sector. Cannot be null.</returns>
        public abstract Sector GetChild(LVector Child);

        /// <summary>
        /// Gets a sector in relation to this sector.
        /// </summary>
        /// <param name="Vector">The vector from this sector to search.</param>
        /// <returns>The sector at the specified relation from this sector. Cannot be null.</returns>
        public virtual Sector GetRelation(LVector Vector)
        {
            LVector size = this.Size;
            Sector parent = this.GetParent(new LVector
            {
                Down = Math.Max(Math.Min(-Vector.Down, size.Down - 1), 0),
                Right = Math.Max(Math.Min(-Vector.Right, size.Right - 1), 0)
            });
            LVector cr = this.ChildRelation;
            LVector diff = cr + Vector;

            if (diff.Down >= 0 && diff.Down < size.Down &&
                diff.Right >= 0 && diff.Right < size.Right)
            {
                // Relation is within the parent sector
                return parent.GetChild(diff);
            }
            else
            {
                int rm = diff.Right % size.Right; rm = rm < 0 ? rm + size.Right : rm;
                int dm = diff.Down % size.Down; dm = dm < 0 ? dm + size.Down : dm;
                LVector ldiff = new LVector(rm, dm);
                LVector sdiff = diff - ldiff; sdiff.Down /= size.Down; sdiff.Right /= size.Right;
                return parent.GetRelation(sdiff).GetChild(ldiff);
            }
        }

        /// <summary>
        /// Gets the parent of this sector. If the parent is not yet created, this will
        /// suggest where to create the parent.
        /// </summary>
        /// <param name="ChildRelation">The relation this will have to its parent if the parent
        /// is not yet created. This is only a suggestion.</param>
        /// <returns>The parent of this sector. Cannot be null.</returns>
        public abstract Sector GetParent(LVector ChildRelation);

        /// <summary>
        /// Returns true if the parent is blank or unassigned.
        /// </summary>
        public abstract bool BlankParent { get; }

        /// <summary>
        /// Gets the parent sector that has this as a child. Cannot be null.
        /// </summary>
        public virtual Sector Parent
        {
            get
            {
                return this.GetParent(new LVector());
            }
        }

        /// <summary>
        /// Gets an enumeration of all children of this sector. May or may
        /// not include unassigned children.
        /// </summary>
        public virtual IEnumerable<Sector> Children
        {
            get
            {
                LVector size = this.Size;
                for (int x = 0; x < size.Right; x++)
                {
                    for (int y = 0; y < size.Down; y++)
                    {
                        yield return this.GetChild(new LVector(x, y));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the amount of deminsions of the child sectors in this sector. Down specifies the amount
        /// of rows of sectors and Up specifies the amount of coloumns of sectors. All sectors in the grid
        /// must have the same size.
        /// </summary>
        public abstract LVector Size { get; }

        /// <summary>
        /// Gets a vector such that this.Parent.GetChild(this.ChildRelation) == this.
        /// </summary>
        public abstract LVector ChildRelation { get; }

        /// <summary>
        /// Gets the location at the center of this sector.
        /// </summary>
        public Location Center
        {
            get
            {
                return new Location() { Sector = this, Offset = { Right = 0.5, Down = 0.5 } };
            }
        }

        /// <summary>
        /// Visual aspects in this sector.
        /// </summary>
        internal Visual.SectorVisData _VisData;
    }
}
