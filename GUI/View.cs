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
        public View(Point Center, double Angle, double Zoom)
        {
            this.Center = Center;
            this.Angle = Angle;
            this.Zoom = Zoom;
        }

        /// <summary>
        /// Sets this view as the current one for future rendering use.
        /// </summary>
        public void Setup(double AspectRatio)
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
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Scale(vsw, vsh, 1.0);
            GL.Rotate(this.Angle * 180 / Math.PI, 0.0, 0.0, 1.0);
            GL.Scale(this.Zoom, this.Zoom, 1.0);
            GL.Translate(-this.Center.X, -this.Center.Y, 0.0);
        }

        /// <summary>
        /// The point in the center of the view.
        /// </summary>
        public Point Center;

        /// <summary>
        /// The rotation of the view, in radians.
        /// </summary>
        public double Angle;

        /// <summary>
        /// The zoom level of the view.
        /// </summary>
        public double Zoom;
    }
}