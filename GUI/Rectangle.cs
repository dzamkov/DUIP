using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace DUIP.GUI
{
    /// <summary>
    /// An axis-aligned rectangle in two-dimensional space.
    /// </summary>
    public struct Rectangle
    {
        public Rectangle(Point TopLeft, Point BottomRight)
        {
            this.TopLeft = TopLeft;
            this.BottomRight = BottomRight;
        }

        public Rectangle(double Left, double Top, double Right, double Bottom)
            : this(new Point(Left, Top), new Point(Right, Bottom))
        {

        }

        /// <summary>
        /// Gets the size of the rectangle.
        /// </summary>
        public Point Size
        {
            get
            {
                return this.BottomRight - this.TopLeft;
            }
        }

        /// <summary>
        /// Gets the total area, in square units, of the rectangle.
        /// </summary>
        public double Area
        {
            get
            {
                return (this.BottomRight.X - this.TopLeft.X) * (this.BottomRight.Y - this.TopLeft.Y);
            }
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the top edge of the rectangle.
        /// </summary>
        public double Top
        {
            get
            {
                return this.TopLeft.Y;
            }
            set
            {
                this.TopLeft.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the bottom edge of the rectangle.
        /// </summary>
        public double Bottom
        {
            get
            {
                return this.BottomRight.Y;
            }
            set
            {
                this.BottomRight.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the left edge of the rectangle.
        /// </summary>
        public double Left
        {
            get
            {
                return this.TopLeft.X;
            }
            set
            {
                this.TopLeft.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the right edge of the rectangle.
        /// </summary>
        public double Right
        {
            get
            {
                return this.BottomRight.X;
            }
            set
            {
                this.BottomRight.X = value;
            }
        }

        /// <summary>
        /// Gets a rectangle which occupies every point on the coordinate plane.
        /// </summary>
        public static Rectangle Unbound
        {
            get
            {
                return new Rectangle(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }
        }

        /// <summary>
        /// Gets a rectangle that does not occupy any points.
        /// </summary>
        public static Rectangle Null
        {
            get
            {
                return new Rectangle(double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);
            }
        }

        /// <summary>
        /// Gets if the rectangle occupies the given point.
        /// </summary>
        public bool Occupies(Point Point)
        {
            return Point.X >= this.Left && Point.X <= this.Right && Point.Y >= this.Top && Point.Y <= this.Bottom;
        }

        /// <summary>
        /// Gets a rectangle covering the intersecting area of A and B.
        /// </summary>
        public static Rectangle Intersection(Rectangle A, Rectangle B)
        {
            return new Rectangle(
                Math.Max(A.Left, B.Left),
                Math.Max(A.Top, B.Top),
                Math.Min(A.Right, B.Right),
                Math.Min(A.Bottom, B.Bottom));
        }

        /// <summary>
        /// The top left (minimum) point on the rectangle.
        /// </summary>
        public Point TopLeft;

        /// <summary>
        /// The bottom right (maximum) point on the rectangle.
        /// </summary>
        public Point BottomRight;
    }
}