using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

using OpenTK.Graphics.OpenGL;
using Color4 = OpenTK.Graphics.Color4;

namespace DUIP.Visual
{
    /// <summary>
    /// Represents a color.
    /// </summary>
    public struct Color
    {
        public Color(byte R, byte G, byte B, byte A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }

        internal Color4 _ToColor4()
        {
            return new Color4(this.R, this.G, this.B, this.A);
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }

    /// <summary>
    /// Representation of a method of filling in 1-d lines or curves. If a pen
    /// is given as null, no curves or lines will be drawn.
    /// </summary>
    public abstract class Pen
    {
        /// <summary>
        /// Draws the list of points.
        /// </summary>
        /// <param name="Points">The points to draw.</param>
        /// <param name="Closed">Is the list of points closed?</param>
        /// <param name="Context">The context this pen is in.</param>
        public abstract void Draw(IEnumerable<Point> Points, bool Closed, Context Context);
    }

    /// <summary>
    /// Representation of a method of filling in polygons.
    /// </summary>
    public abstract class Brush
    {
        /// <summary>
        /// Draws the specified triangles.
        /// </summary>
        /// <param name="Triangles">The triangles to draw.</param>
        /// <param name="Context">The context the brush is in.</param>
        public abstract void Draw(IEnumerable<Triangle> Triangles, Context Context);
    }

    /// <summary>
    /// Represents a point on a drawing surface.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Gets a vector representation of the point with the only requirement being that
        /// the vector must be related to the vectors of other points in the context.
        /// </summary>
        public SVector Vector
        {
            get
            {
                return this._Pos;
            }
        }

        internal SVector _Pos;
    }

    /// <summary>
    /// Parameters needed for drawing.
    /// </summary>
    public class Context
    {
        internal Context(SectorTransform Trans, Bounds PointBounds)
        {
            this._Trans = Trans;
            this._PointBounds = PointBounds;
        }

        /// <summary>
        /// Draws a polygon in this context.
        /// </summary>
        /// <param name="Polygon">The polygon to draw.</param>
        /// <param name="Pen">The pen to use to draw the outline of the polygon, or null to
        /// not draw an outline.</param>
        /// <param name="Brush">The brush to use to fill the polygon, or null to not fill
        /// the polygon.</param>
        public void Draw(Polygon Polygon, Pen Pen, Brush Brush)
        {
            Polygon simple = Polygon.Simplify(100.0);
            IEnumerable<Point> points = Polygon.PointList;
            IEnumerable<Triangle> tris = Polygon.TriangularDecomposition();
            if (Pen != null) Pen.Draw(points, Polygon.Closed, this);
            if (Brush != null) Brush.Draw(tris, this);
        }

        /// <summary>
        /// Creates a solid brush of a single color.
        /// </summary>
        /// <param name="Color">The color of the brush.</param>
        /// <returns>A solid color brush with the specified color.</returns>
        public SolidBrush CreateSolidBrush(Color Color)
        {
            return new SolidBrush(Color);
        }

        /// <summary>
        /// Insure the point is within this context's bounds. If a points is not in the bounds,
        /// it can not be rendered.
        /// </summary>
        /// <param name="Point">The point to check.</param>
        /// <returns>True if the point is within bounds.</returns>
        internal bool _CheckBounds(Point Point)
        {
            if (Point._Pos.Right >= this._PointBounds.TopLeft.Right &&
                Point._Pos.Right <= this._PointBounds.BottomRight.Right &&
                Point._Pos.Down >= this._PointBounds.TopLeft.Down &&
                Point._Pos.Down <= this._PointBounds.BottomRight.Down)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Transforms the point to a space that can be transfered to opengl.
        /// </summary>
        /// <param name="Point">The point to transform.</param>
        /// <returns>The point in screenspace.</returns>
        internal SVector _Transform(Point Point)
        {
            SVector vec = Point._Pos;
            this._Trans.Transform(ref vec);
            return vec;
        }

        /// <summary>
        /// The relative bounds where points and drawings can be placed in this context.
        /// </summary>
        private Bounds _PointBounds;

        /// <summary>
        /// Transform to use before sending the points to opengl.
        /// </summary>
        private SectorTransform _Trans;
    }

    /// <summary>
    /// Solid-color brush.
    /// </summary>
    public class SolidBrush : Brush
    {
        internal SolidBrush(Color Color)
        {
            this._Color = Color;
        }

        public override void Draw(IEnumerable<Triangle> Triangles, Context Context)
        {
            GL.Begin(BeginMode.Triangles);
            foreach (Triangle t in Triangles)
            {
                foreach (Point p in t.PointList)
                {
                    SVector vec = Context._Transform(p);
                    GL.Color4(this._Color._ToColor4());
                    GL.Vertex3(vec.Right, vec.Down, -0.1);
                }
            }
            GL.End();
        }

        /// <summary>
        /// Gets or sets the color of this brush.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                this._Color = value;
            }
        }

        private Color _Color;
    }
}
