﻿//***********************************
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
    public class Sector
    {
        private Sector(World World)
        {
            this._World = World;
            int rc = this._World.RelationSize;
            this._Children = new Sector[this.Size.Right, this.Size.Down];
            this._RelationCache = new Sector[rc * 2 + 1, rc * 2 + 1];
        }

        /// <summary>
        /// Gets a child sector of this sector.
        /// </summary>
        /// <param name="Child">The vector that specifies the child to
        /// get. This vector is in relation to the child at the top-left corner
        /// of this sector.</param>
        /// <returns>The specified child sector. Cannot be null.</returns>
        public Sector GetChild(LVector Child)
        {
            Sector child = this._Children[Child.Right, Child.Down];
            if (child == null)
            {
                child = this._Children[Child.Right, Child.Down] = new Sector(this._World);
                child._Parent = this;
                child._ChildRelation = Child;
                child._Init();
            }
            return child;
        }

        /// <summary>
        /// Gets a sector in relation to this sector.
        /// </summary>
        /// <param name="Vector">The vector from this sector to search.</param>
        /// <returns>The sector at the specified relation from this sector. Cannot be null.</returns>
        public Sector GetRelation(LVector Vector)
        {
            int rc = this._World.RelationSize;
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
                    return this._RelationCache[diff.Right, diff.Down] = this._GetRelation(Vector);
                }
            }
            return this._GetRelation(Vector);
        }

        /// <summary>
        /// Gets a relation without caching.
        /// </summary>
        /// <param name="Vector">The vector for the relation.</param>
        /// <returns>The sector at the specified relation</returns>
        private Sector _GetRelation(LVector Vector)
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
        public Sector GetParent(LVector ChildRelation)
        {
            if (this._Parent == null)
            {
                this._Parent = new Sector(this._World);
                this._ChildRelation = ChildRelation;
                this._Parent._Children[ChildRelation.Right, ChildRelation.Down] = this;
                this._Parent._Init();
            }
            return this._Parent;
        }

        /// <summary>
        /// Returns true if the parent is blank or unassigned.
        /// </summary>
        public bool BlankParent
        {
            get
            {
                return this._Parent == null;
            }
        }

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
        public IEnumerable<Sector> Children
        {
            get
            {
                for (int x = 0; x < this.Size.Right; x++)
                {
                    for (int y = 0; y < this.Size.Down; y++)
                    {
                        Sector gs = this._Children[x, y];
                        if (gs != null)
                        {
                            yield return gs;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the amount of deminsions of the child sectors in this sector. Down specifies the amount
        /// of rows of sectors and Up specifies the amount of coloumns of sectors. All sectors in the grid
        /// must have the same size.
        /// </summary>
        public LVector Size
        {
            get
            {
                return this._World.SectorSize;
            }
        }

        /// <summary>
        /// Gets a vector such that this.Parent.GetChild(this.ChildRelation) == this.
        /// </summary>
        public LVector ChildRelation
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
        /// Creates a blank unfilled general sector.
        /// </summary>
        /// <param name="World">The world this unit is for.</param>
        /// <returns>A new general sector.</returns>
        public static Sector Create(World World)
        {
            Sector gs = new Sector(World);
            gs._Init();
            return gs;
        }

        /// <summary>
        /// Initializes the sector after its relations are set up.
        /// </summary>
        private void _Init()
        {
            this._VisData = new Visual.SectorVisData(this);
        }

        private World _World;
        private Sector[,] _Children;
        private Sector[,] _RelationCache;
        private Sector _Parent;
        private LVector _ChildRelation;
        internal Visual.SectorVisData _VisData;
    }
}
