using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// A description of a renderable image on an infinitely large plane.
    /// </summary>
    public abstract class Figure
    {
        /// <summary>
        /// Gets a figure with a solid color at all points.
        /// </summary>
        public static SolidFigure Solid(Color Color)
        {
            return new SolidFigure(Color);
        }

        /// <summary>
        /// Renders the figure to the current graphics context when the given view is used.
        /// </summary>
        public abstract void Render(View View);

        /// <summary>
        /// Gets a rectangle such that all points outside the rectangle are completely transparent (have an
        /// alpha value of 0).
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Unbound;
            }
        }
    }

    /// <summary>
    /// A figure with a solid color at all points.
    /// </summary>
    public class SolidFigure : Figure
    {
        public SolidFigure(Color Color)
        {
            this._Color = Color;
        }

        /// <summary>
        /// Gets the solid color used.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        public override void Render(View View)
        {
            throw new NotImplementedException();
        }

        private Color _Color;
    }
}