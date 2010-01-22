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
    /// Represents the transform between a set of sectors. The transform can have partial
    /// offsets and can transform any point on a sector to another. This type of transform
    /// is not exact.
    /// </summary>
    public struct STransform
    {
        public STransform(SVector Offset, SVector Scale)
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
        public STransform Append(STransform Transform)
        {
            STransform st = new STransform();
            st.Offset = this.Offset + new SVector(this.Scale.Right * Transform.Offset.Right, this.Scale.Down * Transform.Offset.Down);
            st.Scale = new SVector(this.Scale.Right * Transform.Scale.Right, this.Scale.Down * Transform.Scale.Down);
            return st;
        }

        /// <summary>
        /// Creates a sector transform that specifies a relation between sectors like the
        /// one created by GetRelation.
        /// </summary>
        /// <param name="Offset">Offset in sector units.</param>
        public static STransform Relation(SVector Offset)
        {
            return new STransform(Offset, new SVector(1, 1));
        }

        /// <summary>
        /// Creates a sector transform to get to a child sector.
        /// </summary>
        /// <param name="Size">The size of the sectors in the grid.</param>
        /// <param name="Child">The location of the child.</param>
        public static STransform Child(LVector Size, LVector Child)
        {
            return new STransform(
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
        public static STransform Parent(LVector Size, LVector ChildRelation)
        {
            return new STransform(
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
    /// Represents the transform between a set of sectors. The transform can only have offsets to 
    /// exact sectors, but is able to represent every transform perfectly.
    /// </summary>
    public struct XTransform
    {
        public XTransform(XTransform Source)
        {
            this._Vecs = null;
            this.Append(Source);
        }

        public XTransform(XTransform Base, XVector Add)
        {
            this._Vecs = null;
            this.Append(Base);
            this.Append(Add);
            this.Optimize();
        }

        public XTransform(BinaryReadStream Stream)
        {
            int amount = Stream.ReadInt();
            if (amount > 0)
            {
                this._Vecs = new List<XVector>();
                for (int t = 0; t < amount; t++)
                {
                    this._Vecs.Add(new XVector(Stream));
                }
            }
            else
            {
                this._Vecs = null;
            }
        }

        public void Serialize(BinaryWriteStream Stream)
        {
            if (this._Vecs == null)
            {
                Stream.WriteInt(0);
            }
            else
            {
                Stream.WriteInt(this._Vecs.Count);
                foreach (XVector vec in this._Vecs)
                {
                    vec.Serialize(Stream);
                }
            }
        }

        /// <summary>
        /// Appends an XVector to this transform.
        /// </summary>
        /// <param name="Vector">The vector to add.</param>
        public void Append(XVector Vector)
        {
            if (this._Vecs == null)
            {
                this._Vecs = new List<XVector>();
            }
            this._Vecs.Add(Vector);
        }

        /// <summary>
        /// Appends another transform to this transform.
        /// </summary>
        /// <param name="Transform">The transform to add.</param>
        public void Append(XTransform Transform)
        {
            if (Transform._Vecs != null)
            {
                if (this._Vecs == null)
                {
                    this._Vecs = new List<XVector>();
                }
                this._Vecs.AddRange(Transform._Vecs);
            }
        }

        /// <summary>
        /// Applies this transform to a sector.
        /// </summary>
        /// <param name="Sector">The sector to transform.</param>
        /// <returns>The resulting sector.</returns>
        public Sector Apply(Sector Sector)
        {
            if (this._Vecs == null)
            {
                return Sector;
            }
            else
            {
                Sector cur = Sector;
                foreach (XVector vec in this._Vecs)
                {
                    cur = vec.Apply(cur);
                }
                return cur;
            }
        }

        /// <summary>
        /// Tries to shorten the amount of data needed to represent this transform.
        /// </summary>
        public void Optimize()
        {
            //TODO: This.
        }

        private List<XVector> _Vecs;
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
        public STransform ToSectorTransform()
        {
            return STransform.Relation(new SVector((double)this.Offset.Right, (double)this.Offset.Down));
        }
    }
}
