using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// An affline transformation that relates a certain world space and view space. When a view is bounded,
    /// only the world space that corresponds to a unit square with the top left corner at the origin is visible.
    /// </summary>
    public struct View
    {
        public View(Point Offset, Point Right, Point Down)
        {
            this.Offset = Offset;
            this.Right = Right;
            this.Down = Down;
        }

        public View(Rectangle ViewRectangle)
        {
            this.Offset = ViewRectangle.TopLeft;
            this.Right = new Point(ViewRectangle.Right - ViewRectangle.Left, 0.0);
            this.Down = new Point(0.0, ViewRectangle.Bottom - ViewRectangle.Top);
        }

        /// <summary>
        /// Gets a view where coordinates are the same in view and world space.
        /// </summary>
        public static View Identity
        {
            get
            {
                return new View(
                    new Point(0.0, 0.0),
                    new Point(1.0, 0.0),
                    new Point(0.0, 1.0));
            }
        }

        /// <summary>
        /// Gets the position in world space of the top left corner of this bounded view.
        /// </summary>
        public Point TopLeft
        {
            get
            {
                return this.Offset;
            }
        }

        /// <summary>
        /// Gets the position in world space of the top right corner of this bounded view.
        /// </summary>
        public Point TopRight
        {
            get
            {
                return this.Offset + this.Right;
            }
        }

        /// <summary>
        /// Gets the position in world space of the bottom left corner of this bounded view.
        /// </summary>
        public Point BottomLeft
        {
            get
            {
                return this.Offset + this.Down;
            }
        }

        /// <summary>
        /// Gets the position in world space of the bottom right corner of this bounded view.
        /// </summary>
        public Point BottomRight
        {
            get
            {
                return this.Offset + this.Right + this.Down;
            }
        }

        /// <summary>
        /// Gets the area in world space for a corresponding unit square in view space.
        /// </summary>
        public double Area
        {
            get
            {
                return this.Right.X * this.Down.Y - this.Right.Y * this.Down.X;
            }
        }

        /// <summary>
        /// Gets the inverse view of this view. The resulting view will have the world space
        /// and view space of this view swapped.
        /// </summary>
        public View Inverse
        {
            get
            {
                double det = 1.0 / (this.Right.X * this.Down.Y - this.Right.Y * this.Down.X);
                return new View(
                    new Point(
                        (this.Down.X * this.Offset.Y - this.Down.Y * this.Offset.X) * det,
                        (this.Right.Y * this.Offset.X - this.Right.X * this.Offset.Y) * det),
                    new Point(this.Down.Y * det, this.Right.Y * -det),
                    new Point(this.Down.X * -det, this.Right.X * det));
            }
        }

        /// <summary>
        /// Composes two view such that a projection with the resulting view will be equivalent to a projection
        /// by A followed by a projection by B.
        /// </summary>
        public static View Compose(View A, View B)
        {
            return new View(
                B.Project(A.Offset),
                B.Right * A.Right.X + B.Down * A.Right.Y,
                B.Right * A.Down.X + B.Down * A.Down.Y);
        }

        /// <summary>
        /// Creates a view that translates projected points by a certain offset.
        /// </summary>
        public static View Translation(Point Offset)
        {
            return new View(Offset, new Point(1.0, 0.0), new Point(0.0, 1.0));
        }

        /// <summary>
        /// Creates a view that multiplies projected points by the given factor.
        /// </summary>
        public static View Scale(double Factor)
        {
            return new View(Point.Origin, new Point(Factor, 0.0), new Point(0.0, Factor));
        }

        /// <summary>
        /// Creates a view that rotates projected points counter-clockwise about the origin by
        /// a certain angle in radians.
        /// </summary>
        public static View Rotation(double Angle)
        {
            double cos = Math.Cos(Angle);
            double sin = Math.Sin(Angle);
            return new View(Point.Origin, new Point(cos, -sin), new Point(sin, cos));
        }

        /// <summary>
        /// Projections a point from view space to world space.
        /// </summary>
        public Point Project(Point View)
        {
            return this.Offset + this.Right * View.X + this.Down * View.Y;
        }

        public static View operator *(View A, View B)
        {
            return View.Compose(A, B);
        }

        /// <summary>
        /// The location of the top-left corner of the view in world space.
        /// </summary>
        public Point Offset;

        /// <summary>
        /// The vector from the left edge of the the view to the corresponding point on the right edge of
        /// the view in world space.
        /// </summary>
        public Point Right;

        /// <summary>
        /// The vector from the top edge of the the view to the corresponding point on the bottom edge of
        /// the view in world space.
        /// </summary>
        public Point Down;
    }
}