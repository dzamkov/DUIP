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
    /// A constrained rectangular area.
    /// </summary>
    internal struct Bounds
    {
        public Bounds(SVector TopLeft, SVector BottomRight)
        {
            this.TopLeft = TopLeft;
            this.BottomRight = BottomRight;
        }

        /// <summary>
        /// Top left point of this bounds.
        /// </summary>
        public SVector TopLeft;

        /// <summary>
        /// Bottom right point of this bounds.
        /// </summary>
        public SVector BottomRight;
    }

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
            Bounds initb = new Bounds(new SVector(tl.X, tl.Y), new SVector(br.X, br.Y));

            // Sectors
            Sector center = View.Location.Sector;
            LVector ltl = initb.TopLeft.ToLVector();
            LVector lbr = initb.BottomRight.ToLVector();
            for (int x = ltl.Right; x <= lbr.Right; x++)
            {
                for (int y = ltl.Down; y <= lbr.Down; y++)
                {
                    LVector rel = new LVector(x, y);
                    SectorTransform trans = SectorTransform.Relation(new SVector((double)rel.Right, (double)rel.Down));
                    Sector sec = center.GetRelation(rel);
                    this._DrawSector(sec, trans, initb);
                }
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
            mat *= Matrix4d.CreateTranslation(Transform.Offset.Right, Transform.Offset.Down, 0.0);
            mat *= Matrix4d.Scale(Transform.Scale.Right, Transform.Scale.Down, 0.0);
            return mat;
        }

        /// <summary>
        /// Draws the specified sector with the transform specified and the screen bounds for clipping.
        /// </summary>
        /// <param name="Sector">The sector to draw.</param>
        /// <param name="Transform">The transform from the view sector and bounds to the sector.</param>
        /// <param name="Bounds"></param>
        public void _DrawSector(Sector Sector, SectorTransform Transform, Bounds Bounds)
        {
            SVector tl = new SVector(0.0, 0.0); Transform.Transform(ref tl);
            SVector br = new SVector(1.0, 1.0); Transform.Transform(ref br);

            Matrix4d trans = this.GetSectorTransform(Transform);

            Context ct = new Context(new Grid(0.0, 0.0, 1.0, 1.0), trans);
            Brush brush = ct.CreateSolidBrush(new Color(0, 0, 0, 255));
            ComplexPolygon poly = new ComplexPolygon();
            Grid maingrid = ct.Grid;
            poly.PushPoint(maingrid.GetRelativePoint(0.2, 0.1));
            poly.PushPoint(maingrid.GetRelativePoint(0.5, 0.4));
            poly.PushPoint(maingrid.GetRelativePoint(0.8, 0.1));
            poly.PushPoint(maingrid.GetRelativePoint(0.9, 0.2));
            poly.PushPoint(maingrid.GetRelativePoint(0.6, 0.5));
            poly.PushPoint(maingrid.GetRelativePoint(0.9, 0.8));
            poly.PushPoint(maingrid.GetRelativePoint(0.8, 0.9));
            poly.PushPoint(maingrid.GetRelativePoint(0.5, 0.6));
            poly.PushPoint(maingrid.GetRelativePoint(0.2, 0.9));
            poly.PushPoint(maingrid.GetRelativePoint(0.1, 0.8));
            poly.PushPoint(maingrid.GetRelativePoint(0.4, 0.5));
            poly.PushPoint(maingrid.GetRelativePoint(0.1, 0.2));
            ct.Draw(poly, null, brush);
        }
    }
}
