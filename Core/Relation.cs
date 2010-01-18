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
        public SectorTransform Append(SectorTransform Transform)
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
        /// Creates a sector transform to get a parent sector.
        /// </summary>
        /// <param name="Size">The size of the sectors in the grid.</param>
        /// <param name="ChildRelation">The location of this relative to the parent.</param>
        public static SectorTransform Parent(LVector Size, LVector ChildRelation)
        {
            return new SectorTransform(
                new SVector(
                    (double)-ChildRelation.Right,
                    (double)-ChildRelation.Down
                ), new SVector(
                    (double)Size.Right,
                    (double)Size.Down));
        }

        /// <summary>
        /// Transforms a vector with this transform.
        /// </summary>
        /// <param name="Vector">The vector to transform.</param>
        public void Transform(ref SVector Vector)
        {
            Vector = this.Offset + new SVector(Vector.Right * this.Scale.Right, Vector.Down * this.Scale.Down);
        }

        /// <summary>
        /// Gets the matrix for this transform.
        /// </summary>
        internal OpenTK.Matrix4d _Matrix
        {
            get
            {
                OpenTK.Matrix4d mat = OpenTK.Matrix4d.Identity;
                mat *= OpenTK.Matrix4d.Scale(this.Scale.Right, this.Scale.Down, 0.0);
                mat *= OpenTK.Matrix4d.CreateTranslation(this.Offset.Right, this.Offset.Down, 0.0);
                return mat;
            }
        }
    }

    /// <summary>
    /// Represents a sector relative to another.
    /// </summary>
    public struct RelativeSector
    {
        public RelativeSector(LVector Offset, Sector Sector)
        {
            this.Offset = Offset;
            this.Sector = Sector;
        }

        /// <summary>
        /// The offset the specified sector has.
        /// </summary>
        public LVector Offset;

        /// <summary>
        /// The sector being pointed to.
        /// </summary>
        public Sector Sector;

        /// <summary>
        /// Reverses the relative sector, giving a relative sector from the target to the
        /// reference.
        /// </summary>
        /// <param name="Reference">The reference sector.</param>
        /// <returns>A RelativeSector from the target to the reference.</returns>
        public RelativeSector Reverse(Sector Reference)
        {
            return new RelativeSector(new LVector(-this.Offset.Right, -this.Offset.Down), Reference);
        }

        /// <summary>
        /// Gets the sector transform that represents the transform from the 
        /// reference sector to this relative sector.
        /// </summary>
        /// <returns>The sector transform representation of this.</returns>
        public SectorTransform ToSectorTransform()
        {
            return SectorTransform.Relation(new SVector((double)this.Offset.Right, (double)this.Offset.Down));
        }
    }
}
