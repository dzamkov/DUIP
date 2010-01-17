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

        }

        /// <summary>
        /// Renders the specified view on to GL.
        /// </summary>
        /// <param name="View">The view to render.</param>
        /// <param name="AspectRatio">Width divided by height of the viewport.</param>
        public void Render(View View, double AspectRatio)
        {
            // Clear
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(255, 255, 255, 255);

            // Set ortho projection.
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            Matrix4d proj = this.GetProjectionMatrix(View, AspectRatio);
            Matrix4d iproj = Matrix4d.Invert(proj);
            GL.LoadMatrix(ref proj);
            

            // Set model transform
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Initial bounds
            Vector3d tl = new Vector3d(-1.0, 1.0, 0.0); Vector3d.Transform(ref tl, ref iproj, out tl);
            Vector3d br = new Vector3d(1.0, -1.0, 0.0); Vector3d.Transform(ref br, ref iproj, out br);
            Grid initb = new Grid(tl.X, tl.Y, br.X, br.Y);

            // Sectors
            Sector center = View.Location.Sector;
            IEnumerable<Section> sections = null;
            foreach (RelativeSector i in initb._GetIntersectedSectors(center))
            {
                LVector rel = i.Offset;
                SectorTransform trans = SectorTransform.Relation(new SVector((double)rel.Right, (double)rel.Down));
                IEnumerable<Section> nsections = this._SectionsInSector(i.Sector, trans);
                if (sections == null)
                {
                    sections = nsections;
                }
                else
                {
                    sections = sections.Concat(nsections);
                }
            }

            // Prepare
            foreach (Section s in sections)
            {
                s._Prepare();
            }

            // Draw
            foreach (Section s in sections)
            {
                s._Draw();
            }

            // Restore
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        /// <summary>
        /// Gets the projection matrix onto the the specified view.
        /// </summary>
        /// <param name="View">The view to create the projection matrix for.</param>
        /// <param name="AspectRatio">The aspect ratio of the viewport.</param>
        /// <returns>The projection matrix that can be used to project an
        /// object on a sector to the viewport.</returns>
        public Matrix4d GetProjectionMatrix(View View, double AspectRatio)
        {
            Matrix4d mat = Matrix4d.Identity;
            mat *= Matrix4d.CreateTranslation(-View.Location.Offset.Right, -View.Location.Offset.Down, 0.0);
            mat *= Matrix4d.Scale(View.ZoomLevel, View.ZoomLevel, 1.0);
            mat *= Matrix4d.Scale(0.5, -0.5, 1.0);
            if (AspectRatio > 1.0)
            {
                mat *= Matrix4d.CreateOrthographic(1.0, 1.0 / AspectRatio, -1.0, 1.0);
            }
            else
            {
                mat *= Matrix4d.CreateOrthographic(1.0 * AspectRatio, 1.0, -1.0, 1.0);
            }
            return mat;
        }

        /// <summary>
        /// Creates a matrix for a sector transform.
        /// </summary>
        /// <param name="SecTrans">The sector transform to use.</param>
        /// <returns>The transformation matrix for the sector transform.</returns>
        public Matrix4d GetSectorTransform(SectorTransform Transform)
        {
            Matrix4d mat = Matrix4d.Identity;
            mat *= Matrix4d.Scale(Transform.Scale.Right, Transform.Scale.Down, 0.0);
            mat *= Matrix4d.CreateTranslation(Transform.Offset.Right, Transform.Offset.Down, 0.0);
            return mat;
        }

        /// <summary>
        /// Gets all the sections in the specified sector.
        /// </summary>
        /// <param name="Sector">The sector to get sections for.</param>
        /// <param name="Transform">The transform from the view sector and bounds to the sector.</param>
        /// <returns>The set of sections in this sector. May contain duplicates.</returns>
        public IEnumerable<Section> _SectionsInSector(Sector Sector, SectorTransform Transform)
        {
            if (Sector._VisData.SubSections > 0)
            {
                SVector tl = new SVector(0.0, 0.0); Transform.Transform(ref tl);
                SVector br = new SVector(1.0, 1.0); Transform.Transform(ref br);
                Matrix4d trans = this.GetSectorTransform(Transform);

                IEnumerable<Section> secs = Sector._VisData.Sections;
                foreach (Section s in secs)
                {
                    s._SetTransform(trans);
                    yield return s;
                }

                //Child sectors
                LVector size = Sector.Size;
                for (int x = 0; x < size.Right; x++)
                {
                    for (int y = 0; y < size.Down; y++)
                    {
                        LVector rel = new LVector(x, y);
                        SectorTransform ctrans = SectorTransform.Child(size, rel);
                        foreach (Section s in this._SectionsInSector(Sector.GetChild(rel), Transform.Append(ref ctrans)))
                        {
                            yield return s;
                        }
                    }
                }
            }
        }
    }
}
