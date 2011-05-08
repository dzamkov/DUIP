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
    /// A (possibly mutable) description of a renderable image on an infinitely large plane.
    /// </summary>
    public abstract class Figure
    {
        /// <summary>
        /// Renders the figure using the given context.
        /// </summary>
        public abstract void Render(RenderContext Context);

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
}