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
            this.Rectangle = Rectangle;
        }

        /// <summary>
        /// The rectangle whose perimeter defines this path.
        /// </summary>
        public readonly Rectangle Rectangle;
    }

    /// <summary>
    /// A straight path between two points.
    /// </summary>
    public sealed class SegmentPath : Path
    {
        public SegmentPath(Point A, Point B)
        {
            this.A = A;
            this.B = B;
        }

        /// <summary>
        /// Gets the first endpoint for this segment.
        /// </summary>
        public readonly Point A;

        /// <summary>
        /// Gets the second endpoint for this segment.
        /// </summary>
        public readonly Point B;
    }
}