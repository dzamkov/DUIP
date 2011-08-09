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
            this.Shape = Shape;
            this.Source = Source;
        }

        /// <summary>
        /// The shape of this figure.
        /// </summary>
        public readonly Shape Shape;

        /// <summary>
        /// The source figure that is confined to a shape.
        /// </summary>
        public readonly Figure Source;
    }
}