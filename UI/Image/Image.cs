using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A mapping of points to colors (with alpha channel).
    /// </summary>
    public interface IImage
    {
        /// <summary>
        /// Gets the color of the image at the given point.
        /// </summary>
        Color GetColor(Point Point);
    }
}