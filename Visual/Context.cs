//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

using OpenTK.Graphics.OpenGL;
using Color4 = OpenTK.Graphics.Color4;
using Matrix4d = OpenTK.Matrix4d;

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
        internal Point(double Right, double Down)
        {
            this._Right = Right;
            this._Down = Down;
        }

        /// <summary>
        /// Gets the distance to the right this point is. This is given without
        /// a context and is only useful relative to other points in the same context.
        /// </summary>
        public double Right
        {
            get
            {
                return this._Right;
            }
        }

        /// <summary>
        /// Gets the distance down this point is. This is given without
        /// a context and is only useful relative to other points in the same context.
        /// </summary>
        public double Down
        {
            get
            {
                return this._Down;
            }
        }

        /// <summary>
        /// Gets this point as an SVector.
        /// </summary>
        internal SVector _Vector
        {
            get
            {
                return new SVector(this._Right, this._Down);
            }
        }

        private double _Right;
        private double _Down;
    }

    /// <summary>
    /// Grid for organizing points.
    /// </summary>
    public class Grid
    {
        internal Grid(double Left, double Up, double Right, double Down)
        {
            this._Left = Left;
            this._Up = Up;
            this._Right = Right;
            this._Down = Down;
        }

        /// <summary>
        /// Gets a point relative to this grid.
        /// </summary>
        /// <param name="Right">The amount to the right of the left edge of the grid to make
        /// the point at. 0.0 will make the point on the left edge. 1.0 will make the
        /// point at the right edge.</param>
        /// <param name="Down">The amount down from the top edge of the grid to make the point.
        /// 0.0 will make the point at the top edge while 1.0 will make the point at the
        /// bottom edge.</param>
        /// <returns>The point relative to this grid.</returns>
        public Point GetRelativePoint(double Right, double Down)
        {
            double width = this._Right - this._Left;
            double height = this._Down - this._Up;
            return new Point(Right * width + this._Left, Down * height + this._Up);
        }

        /// <summary>
        /// Creates a subsection grid of this grid.
        /// </summary>
        /// <param name="Left">The relative location of the left edge of the new grid.</param>
        /// <param name="Up">The relative location of the top edge of the new grid.</param>
        /// <param name="Right">The relative location of the right edge of the new grid.</param>
        /// <param name="Down">The relative location of the bottom edge of the new grid.</param>
        /// <returns>The specified subsection grid.</returns>
        public Grid SubGrid(double Left, double Up, double Right, double Down)
        {
            double width = this._Right - this._Left;
            double height = this._Down - this._Up;
            return new Grid(
                Left * width + this._Left,
                Up * height + this._Up,
                Right * width + this._Left,
                Down * height + this._Up);
        }

        /// <summary>
        /// Gets if the specified point is in this grid.
        /// </summary>
        /// <param name="Point">The point to check.</param>
        /// <returns>True if the point is in this grid, false otherwise.</returns>
        public bool InGrid(Point Point)
        {
            return Point.Down >= this._Up && Point.Down <= this._Down && Point.Right >= this._Left && Point.Right <= this._Right;
        }

        /// <summary>
        /// Gets if the specified grid is fully contained in this grid.
        /// </summary>
        /// <param name="Grid">The grid to check against.</param>
        /// <returns>True if the grid is contained in this, false otherwise.</returns>
        public bool Contains(Grid Grid)
        {
            return Grid._Left >= this._Left && Grid._Right <= this._Right && Grid._Up >= this._Up && Grid._Down <= this._Down;
        }

        internal SVector _TopLeft
        {
            get
            {
                return new SVector(this._Left, this._Up);
            }
            set
            {
                this._Left = value.Right;
                this._Up = value.Down;
            }
        }

        internal SVector _BottomRight
        {
            get
            {
                return new SVector(this._Right, this._Down);
            }
            set
            {
                this._Right = value.Right;
                this._Down = value.Down;
            }
        }

        /// <summary>
        /// Gets the sectors this grid intersects if the topleft corner of reference was
        /// (0.0, 0.0).
        /// </summary>
        /// <param name="Reference">The sector to use for a reference.</param>
        /// <returns>An enumeration of sectors this grid intersects paired with the offset of the
        /// sectors from the reference.</returns>
        internal IEnumerable<RelativeSector> _GetIntersectedSectors(Sector Reference)
        {
            LVector ltl = this._TopLeft.ToLVector();
            LVector lbr = this._BottomRight.ToLVector();
            for (int x = ltl.Right; x <= lbr.Right; x++)
            {
                for (int y = ltl.Down; y <= lbr.Down; y++)
                {
                    LVector rel = new LVector(x, y);
                    Sector sec = Reference.GetRelation(rel);
                    yield return new RelativeSector(rel, sec);
                }
            }
        }

        /// <summary>
        /// Gets if this grid intersects would intersect a sector with the specified
        /// offset.
        /// </summary>
        /// <param name="Other">The offset of the sector to test against.</param>
        /// <returns>True if this intersects, false otherwise.</returns>
        internal bool _Intersects(LVector Offset)
        {
            LVector ltl = this._TopLeft.ToLVector();
            LVector lbr = this._BottomRight.ToLVector();
            return Offset.Right >= ltl.Right && Offset.Right <= lbr.Right && Offset.Down >= ltl.Down && Offset.Down <= lbr.Down;
        }

        private double _Left;
        private double _Up;
        private double _Right;
        private double _Down;
    }

    /// <summary>
    /// Parameters needed for drawing.
    /// </summary>
    public class Context
    {
        internal Context(Grid Bounds, Matrix4d Transform)
        {
            this._Transform = Transform;
            this._Bounds = Bounds;
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
            this._ApplyTransform();
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
        /// Gets the grid for this context that represents the area where drawing can
        /// be done.
        /// </summary>
        public Grid Grid
        {
            get
            {
                return this._Bounds;
            }
        }

        /// <summary>
        /// Insure the point is within this context's bounds. If a points is not in the bounds,
        /// it can not be rendered.
        /// </summary>
        /// <param name="Point">The point to check.</param>
        /// <returns>True if the point is within bounds.</returns>
        internal bool _CheckBounds(Point Point)
        {
            return this._Bounds.InGrid(Point);
        }

        /// <summary>
        /// Applies the transform of this context to opengl.
        /// </summary>
        internal void _ApplyTransform()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref this._Transform);
        }

        /// <summary>
        /// Updates the transform of this context.
        /// </summary>
        /// <param name="Transform">The new transform to use.</param>
        internal void _ChangeTransform(Matrix4d Transform)
        {
            this._Transform = Transform;
        }

        /// <summary>
        /// The point bounds of this grid and the
        /// </summary>
        private Grid _Bounds;

        /// <summary>
        /// The transformation the context should use.
        /// </summary>
        private Matrix4d _Transform;
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
                    GL.Color4(this._Color._ToColor4());
                    GL.Vertex3(p.Right, p.Down, -0.1);
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
