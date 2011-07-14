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
            this._Rectangle = Rectangle;
        }

        /// <summary>
        /// Gets the rectangle that defines this shape.
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
    /// A shape that contains all points within some distance of a path in a direction normal to it.
    /// </summary>
    public sealed class PathShape : Shape
    {
        public PathShape(double Thickness, Path Path)
        {
            this._Thickness = Thickness;
            this._Path = Path;
        }

        /// <summary>
        /// Gets the thickness of the path shape.
        /// </summary>
        public double Thickness
        {
            get
            {
                return this._Thickness;
            }
        }

        /// <summary>
        /// Gets the path that defines this shape.
        /// </summary>
        public Path Path
        {
            get
            {
                return this._Path;
            }
        }

        private double _Thickness;
        private Path _Path;
    }
}