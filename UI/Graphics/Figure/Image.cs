using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A figure that displays a rectangular section of an image.
    /// </summary>
    public class ImageFigure : Figure
    {
        public ImageFigure(Image Image, Rectangle Source, Rectangle Destination, Filter Filter)
        {
            this.Image = Image;
            this.Source = Source;
            this.Destination = Destination;
            this.Filter = Filter;
        }

        /// <summary>
        /// The image displayed by this figure.
        /// </summary>
        public readonly Image Image;

        /// <summary>
        /// The source rectangle of the figure. This is given in coordinates relative to the image
        /// with (0.0, 0.0) being the top-left corner and (1.0, 1.0) being the bottom-right corner. The unit square
        /// rectangle specifies that the entire image should be used.
        /// </summary>
        /// <remarks>The image wraps such that the color at (x + k, y + l) is the same at (x, y) 
        /// for any integers k and l.</remarks>
        public readonly Rectangle Source;

        /// <summary>
        /// The destination rectangle for the figure. The contents of the image at source rectangle is shown
        /// in this rectangle.
        /// </summary>
        public readonly Rectangle Destination;

        /// <summary>
        /// Gets the filtering method used for showing the image.
        /// </summary>
        public readonly Filter Filter;
    }

    /// <summary>
    /// Identifies an interpolation method to find the colors between pixels in an image.
    /// </summary>
    public enum Filter
    {
        /// <summary>
        /// The color of a point is determined by the color of the nearest pixel.
        /// </summary>
        Nearest,

        /// <summary>
        /// The color of a point is determined by linearly interpolating between pixels in both directions.
        /// </summary>
        Linear,
    }
}