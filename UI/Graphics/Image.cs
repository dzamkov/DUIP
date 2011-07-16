using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A finite two-dimensional array of colored pixels (including an alpha channel).
    /// </summary>
    public abstract class Image
    {
        /// <summary>
        /// Gets the size of this image.
        /// </summary>
        public abstract void GetSize(out int Width, out int Height);

        /// <summary>
        /// Gets the color for the pixel at the given coordinates.
        /// </summary>
        public abstract Color GetPixel(int X, int Y);
    }
}