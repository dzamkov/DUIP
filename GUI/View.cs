using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// Represents a view of a 2d area.
    /// </summary>
    public struct View
    {
        public View(Point Center, double Zoom)
        {
            this.Center = Center;
            this.Zoom = Zoom;
        }

        /// <summary>
        /// Sets this view as the current one for future rendering use.
        /// </summary>
        public void Setup(double AspectRatio)
        {
            Point scale = this._GetScaleFactor(AspectRatio);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Scale(1.0, -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Scale(scale.X, scale.Y, 1.0);
            GL.Translate(-this.Center.X, -this.Center.Y, 0.0);
        }

        /// <summary>
        /// Gets the rectangular area this view can see (given the aspect ratio).
        /// </summary>
        public Rectangle GetBounds(double AspectRatio)
        {
            Point sf = this._GetScaleFactor(AspectRatio);
            sf.X = 1.0 / sf.X;
            sf.Y = 1.0 / sf.Y;
            return new Rectangle(
                this.Center - sf,
                this.Center + sf);
        }

        /// <summary>
        /// Gets the amount each axis is scaled by from viewspace to worldspace with the given aspect ratio.
        /// </summary>
        private Point _GetScaleFactor(double AspectRatio)
        {
            double vsw;
            double vsh;
            if (AspectRatio > 1.0)
            {
                vsw = 1.0 / AspectRatio;
                vsh = 1.0;
            }
            else
            {
                vsh = AspectRatio;
                vsw = 1.0;
            }
            return new Point(vsw, vsh) * this.Zoom;
        }

        /// <summary>
        /// The point in the center of the view.
        /// </summary>
        public Point Center;

        /// <summary>
        /// The zoom level of the view. 
        /// </summary>
        public double Zoom;
    }
}