using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A figure confined to a shape with all points outside the shape being transparent.
    /// </summary>
    public sealed class ShapeFigure : Figure
    {
        public ShapeFigure(Shape Shape, Figure Source)
        {
            this._Shape = Shape;
            this._Source = Source;
        }

        /// <summary>
        /// Gets the shape of this figure.
        /// </summary>
        public Shape Shape
        {
            get
            {
                return this._Shape;
            }
        }

        /// <summary>
        /// Gets the source figure that is confined to a shape.
        /// </summary>
        public Figure Source
        {
            get
            {
                return this._Source;
            }
        }

        private Shape _Shape;
        private Figure _Source;
    }
}