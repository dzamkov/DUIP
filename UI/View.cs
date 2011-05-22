using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// Represents a view of a two-dimensional area.
    /// </summary>
    public struct View
    {
        public View(Rectangle Area)
        {
            this.Area = Area;
        }

        /// <summary>
        /// Sets this view as the current one for future rendering use.
        /// </summary>
        public void Setup(bool InvertY)
        {
            Point size = this.Area.Size;
            Point center = this.Area.TopLeft + size * 0.5;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Scale(2.0 / size.X, InvertY ? -2.0 / size.Y : 2.0 / size.Y, 1.0);
            GL.Translate(-center.X, -center.Y, 0.0);
        }

        /// <summary>
        /// Projections a point from view-space to world-space.
        /// </summary>
        public Point Project(Point View)
        {
            return this.Area.TopLeft + this.Area.Size.Scale(View);
        }

        /// <summary>
        /// Estimates the zoom level of the view.
        /// </summary>
        public double Zoom
        {
            get
            {
                Point size = this.Area.Size;
                double el = Math.Sqrt(size.X * size.Y);
                return Math.Log(el, 2.0) - 1.0;
            }
        }

        /// <summary>
        /// The area visible by the view.
        /// </summary>
        public Rectangle Area;
    }

    /// <summary>
    /// Stores the location, velocity and zoom level of a viewer of a two-dimensional area.
    /// </summary>
    public struct Camera
    {
        public Camera(Point Center, double Zoom)
        {
            this.Center = Center;
            this.Zoom = Zoom;
            this.Velocity = new Point(0.0, 0.0);
            this.ZoomVelocity = 0.0;
        }

        /// <summary>
        /// Gets the view for this camera using a viewport of the given size.
        /// </summary>
        public View GetView(int Width, int Height)
        {
            double ar = (double)Width / Height; // Aspect ratio
            double size = this.Scale;
            Point off = ar > 1.0 ? new Point(size * ar, size) : new Point(size, size / ar);
            Rectangle rect = new Rectangle(this.Center - off, this.Center + off);
            return new View(rect);
        }

        /// <summary>
        /// Updates the state of the camera by the given amount of time.
        /// </summary>
        /// <param name="Damping">The relative amount of velocity that persists after a time unit.</param>
        public void Update(double Time, double Damping, double MinZoom, double MaxZoom)
        {
            
            this.Center += this.Velocity * this.Scale * Time;
            this.Zoom += this.ZoomVelocity * Time;

            if (this.Zoom < MinZoom)
            {
                this.Zoom = MinZoom;
                this.ZoomVelocity = 0.0;
            }
            if (this.Zoom > MaxZoom)
            {
                this.Zoom = MaxZoom;
                this.ZoomVelocity = 0.0;
            }

            Damping = Math.Pow(Damping, Time);
            this.Velocity *= Damping;
            this.ZoomVelocity *= Damping;
        }

        /// <summary>
        /// Adjusts velocity to zoom while having the given target point remain stationary in the view of the camera.
        /// </summary>
        public void ZoomTo(Point Target, double Factor)
        {
            this.ZoomVelocity += Factor;
            this.Velocity += (this.Center - Target) * (0.7 * Factor / this.Scale);
        }

        /// <summary>
        /// Gets a relative indicator of the amount of space in one direction is visible by the camera.
        /// </summary>
        public double Scale
        {
            get
            {
                return Math.Pow(2.0, this.Zoom);
            }
        }

        /// <summary>
        /// The point in the center of the view.
        /// </summary>
        public Point Center;

        /// <summary>
        /// The zoom level of the camera.
        /// </summary>
        public double Zoom;

        /// <summary>
        /// The velocity of the lateral movement of the camera relative to the scale of the camera.
        /// </summary>
        public Point Velocity;

        /// <summary>
        /// The velocity of the zoom level of the camera.
        /// </summary>
        public double ZoomVelocity;
    }
}