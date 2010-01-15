using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUIP.Core
{
    /// <summary>
    /// Represents a long distance across sectors. The units, given in integers
    /// are the amount of sectors to the right and down the grid.
    /// </summary>
    public struct LVector
    {
        public LVector(int Right, int Down)
        {
            this.Right = Right;
            this.Down = Down;
        }

        public int Right;
        public int Down;

        public static LVector operator +(LVector A, LVector B)
        {
            return new LVector { Down = A.Down + B.Down, Right = A.Right + B.Right };
        }

        public static LVector operator -(LVector A, LVector B)
        {
            return new LVector { Down = A.Down - B.Down, Right = A.Right - B.Right };
        }
    }

    /// <summary>
    /// Represents a precise distance across sectors. The units are the amount of
    /// sectors to the right and down the grid. Units may be fractional and cover
    /// a part of a sector. When used within a sector, (0,0) points to the top-left
    /// corner of the sector and (1,1) points to the bottom-left corner.
    /// </summary>
    public struct SVector
    {
        public SVector(double Right, double Down)
        {
            this.Right = Right;
            this.Down = Down;
        }

        public double Right;
        public double Down;

        /// <summary>
        /// Converts the SVector to an LVector by removing units within a sector.
        /// </summary>
        /// <returns>A less-precise LVector representation of this vector.</returns>
        public LVector ToLVector()
        {
            LVector lv = new LVector();
            lv.Down = (int)(Math.Floor(this.Down));
            lv.Right = (int)(Math.Floor(this.Right));
            return lv;
        }

        public static SVector operator +(SVector A, SVector B)
        {
            return new SVector { Down = A.Down + B.Down, Right = A.Right + B.Right };
        }

        public static SVector operator -(SVector A, SVector B)
        {
            return new SVector { Down = A.Down - B.Down, Right = A.Right - B.Right };
        }

        public static SVector operator +(SVector A, LVector B)
        {
            return new SVector { Down = A.Down + (double)B.Down, Right = A.Right + (double)B.Right };
        }

        public static SVector operator -(SVector A, LVector B)
        {
            return new SVector { Down = A.Down - (double)B.Down, Right = A.Right - (double)B.Right };
        }
    }

    /// <summary>
    /// Represents the transform between a set of sector.
    /// </summary>
    public struct SectorTransform
    {
        public SectorTransform(SVector Offset, SVector Scale)
        {
            this.Offset = Offset;
            this.Scale = Scale;
        }

        /// <summary>
        /// The offset in terms of location of the transform.
        /// </summary>
        public SVector Offset;

        /// <summary>
        /// The difference in scale of the transform.
        /// </summary>
        public SVector Scale;

        /// <summary>
        /// Appends the specified transform to this transform. For example if A
        /// specifies the transform from sector X to sector Y, and B specifies the
        /// transform from Y to Z, A.Append(B) specifies the transform from X to Z.
        /// </summary>
        /// <param name="Transform">The transform to append.</param>
        /// <returns>A sector transform that describes a combination of the specified transforms.</returns>
        public SectorTransform Append(ref SectorTransform Transform)
        {
            SectorTransform st = new SectorTransform();
            st.Offset = this.Offset + new SVector(this.Scale.Right * Transform.Offset.Right, this.Scale.Down * Transform.Offset.Down);
            st.Scale = new SVector(this.Scale.Right * Transform.Scale.Right, this.Scale.Down * Transform.Scale.Down);
            return st;
        }

        /// <summary>
        /// Creates a sector transform that specifies a relation between sectors like the
        /// one created by GetRelation.
        /// </summary>
        /// <param name="Offset">Offset in sector units.</param>
        public static SectorTransform Relation(SVector Offset)
        {
            return new SectorTransform(Offset, new SVector(1, 1));
        }

        /// <summary>
        /// Creates a sector transform to get to a child sector.
        /// </summary>
        /// <param name="Size">The size of the sectors in the grid.</param>
        /// <param name="Child">The location of the child.</param>
        public static SectorTransform Child(LVector Size, LVector Child)
        {
            return new SectorTransform(
                new SVector(
                    (double)Child.Right / (double)Size.Right,
                    (double)Child.Down / (double)Size.Down
                ), new SVector(
                    1.0 / (double)Size.Right,
                    1.0 / (double)Size.Down));

        }

        /// <summary>
        /// Transforms a vector with this transform.
        /// </summary>
        /// <param name="Vector">The vector to transform.</param>
        public void Transform(ref SVector Vector)
        {
            Vector = this.Offset + new SVector(Vector.Right * this.Scale.Right, Vector.Down * this.Scale.Down);
        }
    }

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
            if (m.Down != 0 && m.Right != 0)
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
    }
}
