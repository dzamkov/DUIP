using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A set of points that define a region in two-dimensional space.
    /// </summary>
    public abstract class Shape
    {

    }

    /// <summary>
    /// A shape for an axis-aligned rectangle.
    /// </summary>
    public sealed class RectangleShape : Shape
    {
        public RectangleShape(Rectangle Rectangle)
        {
            this.Rectangle = Rectangle;
        }

        /// <summary>
        /// The rectangle that defines this shape.
        /// </summary>
        public readonly Rectangle Rectangle;
    }

    /// <summary>
    /// A shape that contains all points within some distance of a path in a direction normal to it.
    /// </summary>
    public sealed class PathShape : Shape
    {
        public PathShape(double Thickness, Path Path)
        {
            this.Thickness = Thickness;
            this.Path = Path;
        }

        /// <summary>
        /// The thickness of the path shape.
        /// </summary>
        public readonly double Thickness;

        /// <summary>
        /// The path that defines this shape.
        /// </summary>
        public readonly Path Path;
    }
}