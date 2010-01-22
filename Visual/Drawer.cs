//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using DUIP.Core;

namespace DUIP.Visual
{

    /// <summary>
    /// Class used to draw a world with the view specified.
    /// </summary>
    internal class Drawer
    {
        public Drawer()
        {
            this._Width = 100;
            this._Height = 100;
            this.UpdateView();
        }

        /// <summary>
        /// Gets or sets the width of the area that can be drawn to in pixels.
        /// </summary>
        public double Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                this._Width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the area that can be drawn to in pixels.
        /// </summary>
        public double Height
        {
            get
            {
                return this._Height;
            }
            set
            {
                this._Height = value;
            }
        }

        /// <summary>
        /// Gets the aspect ratio of this drawer, which is width divided by height.
        /// </summary>
        public double AspectRatio
        {
            get
            {
                return this._Width / this._Height;
            }
        }

        /// <summary>
        /// Gets or sets the view this drawer shows.
        /// </summary>
        public View View
        {
            get
            {
                return this._View;
            }
            set
            {
                this._View = value;
            }
        }

        /// <summary>
        /// Called to update the view, width and height of the draw after
        /// any of those are changed.
        /// </summary>
        public void UpdateView()
        {
            // Get matricies
            this._Proj = this.GetProjectionMatrix();
            this._IProj = Matrix4d.Invert(this._Proj);

            // Get bounds
            Vector3d tl = new Vector3d(-1.0, 1.0, 0.0); Vector3d.Transform(ref tl, ref this._IProj, out tl);
            Vector3d br = new Vector3d(1.0, -1.0, 0.0); Vector3d.Transform(ref br, ref this._IProj, out br);
            this._Bounds = new Grid(tl.X, tl.Y, br.X, br.Y);

            // Get sectors
            this._CentralSector = this._View.Location.Sector;
            if (this._CentralSector != null)
            {
                this._Sections = null;
                foreach (RelativeSector i in this._Bounds._GetIntersectedSectors(this._CentralSector))
                {
                    LVector rel = i.Offset;
                    SectorDrawInfo sdi = new SectorDrawInfo();
                    sdi.Sector = i.Sector;
                    sdi.SectorTransform = i.ToSectorTransform();
                    sdi.Size = 0;
                    if (this._Sections == null)
                    {
                        this._Sections = sdi.Sections;
                    }
                    else
                    {
                        this._Sections = this._Sections.Concat(sdi.Sections);
                    }
                }
            }
            else
            {
                this._Sections = new List<Section>();
            }
        }

        /// <summary>
        /// Draws the contents of the drawer to GL.
        /// </summary>
        public void Draw()
        {
            // Clear
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(255, 255, 255, 255);

            // Set ortho projection.
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            Matrix4d proj = this.GetProjectionMatrix();
            Matrix4d iproj = Matrix4d.Invert(proj);
            GL.LoadMatrix(ref proj);


            // Set model transform
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            
            // Prepare
            foreach (Section s in this._Sections)
            {
                s._Prepare();
            }

            // Draw
            foreach (Section s in this._Sections)
            {
                s._Draw();
            }

            // Restore
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        /// <summary>
        /// Gets the projection matrix for this drawer
        /// </summary>
        public Matrix4d GetProjectionMatrix()
        {
            Matrix4d mat = Matrix4d.Identity;
            mat *= Matrix4d.CreateTranslation(-this._View.Location.Offset.Right, -this._View.Location.Offset.Down, 0.0);
            mat *= Matrix4d.Scale(this._View.ZoomLevel, this._View.ZoomLevel, 1.0);
            mat *= Matrix4d.Scale(0.5, -0.5, 1.0);
            double ar = this.AspectRatio;
            if (ar > 1.0)
            {
                mat *= Matrix4d.CreateOrthographic(1.0, 1.0 / ar, -1.0, 1.0);
            }
            else
            {
                mat *= Matrix4d.CreateOrthographic(1.0 * ar, 1.0, -1.0, 1.0);
            }
            return mat;
        }

        /// <summary>
        /// Information used to draw a sector.
        /// </summary>
        private struct SectorDrawInfo
        {
            /// <summary>
            /// Transform from the central sector.
            /// </summary>
            public STransform SectorTransform;

            /// <summary>
            /// The sector this information is for.
            /// </summary>
            public Sector Sector;

            /// <summary>
            /// 0 indicates this is the same size as the central sector.
            /// 1 indicates this is larger than the central sector.
            /// -1 indicates this is smaller than the central sector.
            /// </summary>
            public int Size;

            /// <summary>
            /// Gets all the sections that affect this sector in front to back order.
            /// </summary>
            public IEnumerable<Section> Sections
            {
                get
                {
                    if ((this.Size > -1) && !this.Sector.BlankParent)
                    {
                        foreach (Section s in this.Parent.Sections)
                        {
                            yield return s;
                        }
                    }
                    foreach (SectorVisData.SectionInfo si in this.Sector._VisData.UniqueSections)
                    {
                        si.Section._SetTransform(this.SectorTransform.Append(si.Sector.ToSectorTransform())._Matrix);
                        yield return si.Section;
                    }
                    if ((this.Size < 1) && this.Sector._VisData.SubSections > 0)
                    {
                        foreach (SectorDrawInfo sdi in this.Children)
                        {
                            foreach (Section s in sdi.Sections)
                            {
                                yield return s;
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the child sector draw info for this.
            /// </summary>
            public IEnumerable<SectorDrawInfo> Children
            {
                get
                {
                    LVector size = this.Sector.Size;
                    for (int x = 0; x < size.Right; x++)
                    {
                        for (int y = 0; y < size.Down; y++)
                        {
                            LVector rel = new LVector(x, y);
                            STransform ctrans = STransform.Child(size, rel);
                            SectorDrawInfo sdi = new SectorDrawInfo();
                            sdi.Sector = this.Sector.GetChild(rel);
                            sdi.Size = -1;
                            sdi.SectorTransform = this.SectorTransform.Append(ctrans);
                            yield return sdi;
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the parent sector draw info for this.
            /// </summary>
            public SectorDrawInfo Parent
            {
                get
                {
                    STransform ptrans = STransform.Parent(this.Sector.Size, this.Sector.ChildRelation);
                    SectorDrawInfo sdi = new SectorDrawInfo();
                    sdi.Sector = this.Sector.Parent;
                    sdi.Size = 1;
                    sdi.SectorTransform = this.SectorTransform.Append(ptrans);
                    return sdi;
                }
            }
        }

        private View _View;
        private double _Width;
        private double _Height;
        private Matrix4d _Proj;
        private Matrix4d _IProj;
        private Grid _Bounds;
        private Sector _CentralSector;
        private IEnumerable<Section> _Sections;
    }
}
