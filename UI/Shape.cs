using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A two-dimensional figure that classifies points in the space as either occupied or unoccupied.
    /// </summary>
    public abstract class Shape
    {
        /// <summary>
        /// Gets a rectanglular area such that all points outside the area are known to be unoccupied.
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Unbound;
            }
        }

        /// <summary>
        /// Gets if the shape occupies the given point.
        /// </summary>
        public abstract bool Occupies(Point Point);

        /// <summary>
        /// Gets a path representing the outer border of the shape.
        /// </summary>
        public abstract Path Border { get; }
    }

    /// <summary>
    /// Describes a method of filling a shape with color.
    /// </summary>
    public abstract class FillStyle
    {

    }
}