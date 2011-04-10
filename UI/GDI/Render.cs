using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;

namespace DUIP.UI.GDI
{
    /// <summary>
    /// A render interface using .Net's GDI+.
    /// </summary>
    public class GDIRenderInterface : RenderInterface
    {
        public GDIRenderInterface(Graphics Graphics)
        {
            this._Graphics = Graphics;
        }

        /// <summary>
        /// Gets the graphics interface used.
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                return this._Graphics;
            }
        }

        public override void Clear(Color Color)
        {
            this._Graphics.Clear(Color);
        }

        public override void Line(Color Color, Point A, Point B, double Thickness)
        {
            using (Pen p = new Pen(Color, (float)Thickness))
            {
                this._Graphics.DrawLine(p, A, B);
            }
        }

        private Graphics _Graphics;
    }
}