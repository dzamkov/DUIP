using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

using Matrix4d = OpenTK.Matrix4d;

namespace DUIP.Visual
{
    /// <summary>
    /// An individually drawn area within the world. A section
    /// can persist across multiple frames and reuse resources in frames.
    /// </summary>
    public abstract class Section
    {

        /// <summary>
        /// Creates resources to be used in the section.
        /// </summary>
        /// <param name="Context">The context to use to create
        /// the resources.</param>
        protected abstract void OnCreate(Context Context);

        /// <summary>
        /// Draws the section to the specified context.
        /// </summary>
        /// <param name="Context">The context to use for drawing.</param>
        protected abstract void OnDraw(Context Context);

        /// <summary>
        /// Removes all the resources the section takes. Can later be recreated.
        /// </summary>
        protected abstract void OnDestroy();

        /// <summary>
        /// Adds a subsection to this section. The subsection will
        /// be drawn after this is drawn and will automatically have
        /// LOD(Level of detail) techniques applied to it.
        /// </summary>
        /// <param name="Section">The section to add as a subsection. The section
        /// must not yet have an owner.</param>
        /// <param name="Bounds">The grid relative to this that will
        /// contain the subsection.</param>
        protected void AddSubSection(Section Section, Grid Bounds)
        {
            if (this._Bounds.Contains(Bounds))
            {
                if (Section._Sector == null)
                {
                    Section._Add(this._Sector, Bounds);
                }
                else
                {
                    throw new Exception("You idiot, the section is already added.");
                }
            }
            else
            {
                throw new Exception("Its not in range, moron.");
            }
        }

        /// <summary>
        /// Changes the sector and grid to be more precise without losing data.
        /// </summary>
        private void _Normalize()
        {
            SVector tl = this._Bounds._TopLeft;
            SVector br = this._Bounds._BottomRight;
            LVector tll = tl.ToLVector();
            if (tll.Right != 0 || tll.Down != 0)
            {
                this._Sector = this._Sector.GetRelation(tll);
                tl = this._Bounds._TopLeft = tl - tll;
                br = this._Bounds._BottomRight = br - tll;
            }
        }

        /// <summary>
        /// Adds the section to sector visdata.
        /// </summary>
        private void _AddVisData()
        {
            foreach (RelativeSector rs in this._Bounds._GetIntersectedSectors(this._Sector))
            {
                rs.Sector._VisData.AddSection(this);
            }
        }

        /// <summary>
        /// Removes the section from sector visdata.
        /// </summary>
        private void _RemoveVisData()
        {
            foreach (RelativeSector rs in this._Bounds._GetIntersectedSectors(this._Sector))
            {
                rs.Sector._VisData.RemoveSection(this);
            }
        }

        /// <summary>
        /// Adds this section to a sector and sets its bounds.
        /// </summary>
        /// <param name="Sector">The sector to add this section to.</param>
        /// <param name="Bounds">The bounds of the section.</param>
        internal void _Add(Sector Sector, Grid Bounds)
        {
            this._Sector = Sector;
            this._Bounds = Bounds;
            this._Normalize();
            this._AddVisData();
        }

        /// <summary>
        /// Draws the section with the specified transform.
        /// </summary>
        internal void _Draw()
        {
            if (!this._Drawn)
            {
                if (this._Context == null)
                {
                    this._SetTransform(Matrix4d.Identity);
                }
                if (!this._Created)
                {
                    this.OnCreate(this._Context);
                    this._Created = true;
                }
                this.OnDraw(this._Context);
                this._Drawn = true;
            }
        }

        /// <summary>
        /// Prepares the section for drawing. Preparing can be called multiple times
        /// before draw and eliminates the possiblity of drawing twice without a prepare
        /// in between.
        /// </summary>
        internal void _Prepare()
        {
            this._Drawn = false;
        }

        /// <summary>
        /// Sets the transform used to draw this section.
        /// </summary>
        /// <param name="Transform">The transform from this sections sectorspace to
        /// screenspace.</param>
        internal void _SetTransform(Matrix4d Transform)
        {
            if (this._Context == null)
            {
                this._Context = new Context(this._Bounds, Transform);
            }
            else
            {
                this._Context._ChangeTransform(Transform);
            }
        }

        private Sector _Sector;
        internal Grid _Bounds;
        private Context _Context;
        private bool _Created;
        private bool _Drawn;
    }

    /// <summary>
    /// Visual data used in sectors.
    /// </summary>
    internal struct SectorVisData
    {
        public SectorVisData(Sector Sector)
        {
            this._UniqueSections = new LinkedList<Section>();
            this._Sector = Sector;
            this._SubSections = 0;
            foreach (Sector c in Sector.Children)
            {
                this._SubSections += c._VisData._SubSections;
            }
        }

        /// <summary>
        /// Adds a unique section to this visdata. The section should have
        /// this as its owner.
        /// </summary>
        /// <param name="Section">The section to add.</param>
        public void AddSection(Section Section)
        {
            this._UniqueSections.AddFirst(Section);
            Sector s = this._Sector;
            while (true)
            {
                s._VisData._SubSections++;
                if (s.BlankParent)
                {
                    break;
                }
                else
                {
                    s = s.Parent;
                }
            }
        }

        /// <summary>
        /// Removes a section that was previously added to this.
        /// </summary>
        /// <param name="Section">The section to remove.</param>
        public void RemoveSection(Section Section)
        {
            this._UniqueSections.Remove(Section);
            Sector s = this._Sector;
            while (true)
            {
                s._VisData._SubSections--;
                if (s.BlankParent)
                {
                    break;
                }
                else
                {
                    s = s.Parent;
                }
            }
        }

        /// <summary>
        /// Gets the amount of sections between this and all its children.
        /// </summary>
        public ulong SubSections
        {
            get
            {
                return this._SubSections;
            }
        }

        /// <summary>
        /// Gets a list of all sections(front to back order) in sectors this size or bigger that
        /// affect this sector.
        /// </summary>
        public IEnumerable<Section> Sections
        {
            get
            {
                if (this._Sector.BlankParent)
                {
                    return this._UniqueSections;
                }
                else
                {
                    return this._UniqueSections.Concat(this._Sector.Parent._VisData.Sections);
                }
            }
        }

        /// <summary>
        /// List of sections that affect this sector but not any parents or children in front to back order.
        /// </summary>
        private LinkedList<Section> _UniqueSections;

        /// <summary>
        /// Total amount of sections between this and all its children.
        /// </summary>
        private ulong _SubSections;

        /// <summary>
        /// The sector this visdata is for.
        /// </summary>
        private Sector _Sector;
    }
}
