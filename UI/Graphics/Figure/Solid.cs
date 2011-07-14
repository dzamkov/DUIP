using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A figure where all points are of a certain color. This is useful in combination with a ShapeFigure to make a solid-colored shape.
    /// </summary>
    public sealed class SolidFigure : Figure
    {
        public SolidFigure(Color Color)
        {
            this._Color = Color;
        }

        /// <summary>
        /// Gets the color of this figure.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        private Color _Color;
    }
}