﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A figure defined by a function mapping points to color.
    /// </summary>
    public abstract class SampledFigure : Figure
    {
        /// <summary>
        /// Gets the color of this figure at the given point.
        /// </summary>
        public abstract Color GetColor(Point Point);

        /// <summary>
        /// Gets the bounds of this figure such that all points outside this rectangle
        /// are completely transparent.
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Unbound;
            }
        }

        /// <summary>
        /// Gets wether this sampled figure is tiled. If so, every point of the form
        /// (x + k, y + l) where k and l are integers and x and y are reals in the interval
        /// [0.0, 1.0) will have the same color as the point (x, y).
        /// </summary>
        public virtual bool Tiled
        {
            get
            {
                return false;
            }
        }
    }
}