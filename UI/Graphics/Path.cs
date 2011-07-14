using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A continous set of points that define a curve in two-dimensional space. Paths may be open or closed.
    /// </summary>
    public abstract class Path
    {

    }

    /// <summary>
    /// A path that follows the perimeter of a axis-aligned rectangle.
    /// </summary>
    public sealed class RectanglePath : Path
    {
        public RectanglePath(Rectangle Rectangle)
        {
            this._Rectangle = Rectangle;
        }

        /// <summary>
        /// Gets the rectangle whose perimeter defines this path.
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return this._Rectangle;
            }
        }

        private Rectangle _Rectangle;
    }

    /// <summary>
    /// A straight path between two points.
    /// </summary>
    public sealed class SegmentPath : Path
    {
        public SegmentPath(Point A, Point B)
        {
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Gets the first endpoint for this segment.
        /// </summary>
        public Point A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets the second endpoint for this segment.
        /// </summary>
        public Point B
        {
            get
            {
                return this._B;
            }
        }

        private Point _A;
        private Point _B;
    }
}