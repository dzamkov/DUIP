using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// Stores the location and resolution of a view of a two-dimensional area.
    /// </summary>
    public struct View
    {
        public View(Rectangle Area, double Resolution)
        {
            this.Area = Area;
            this.Resolution = Resolution;
        }

        /// <summary>
        /// Sets this view as the current one for future rendering use.
        /// </summary>
        public void Setup()
        {
            Point size = this.Area.Size;
            Point center = this.Area.TopLeft + size * 0.5;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Scale(2.0 / size.X, -2.0 / size.Y, 1.0);
            GL.Translate(-center.X, -center.Y, 0.0);
        }

        /// <summary>
        /// The area visible by the view.
        /// </summary>
        public Rectangle Area;

        /// <summary>
        /// The amount of pixels for a 1.0 * 1.0 square area in the view.
        /// </summary>
        public double Resolution;
    }

    /// <summary>
    /// Stores the location and zoom level of a viewer of a two-dimensional area.
    /// </summary>
    public struct Camera
    {
        public Camera(Point Center, double Zoom)
        {
            this.Center = Center;
            this.Size = Zoom;
        }

        /// <summary>
        /// Gets the view for this camera using a viewport of the given size.
        /// </summary>
        public View GetView(int Width, int Height)
        {
            double ar = (double)Width / Height; // Aspect ratio
            Point off = ar > 1.0 ? new Point(this.Size * ar, this.Size) : new Point(this.Size, this.Size / ar);
            Rectangle rect = new Rectangle(this.Center - off, this.Center + off);
            double res = (double)Width / off.X * (double)Height / off.Y / 4.0;
            return new View(rect, res);
        }

        /// <summary>
        /// The point in the center of the view.
        /// </summary>
        public Point Center;

        /// <summary>
        /// The extent of the viewable area.
        /// </summary>
        public double Size;
    }
}