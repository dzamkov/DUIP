using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Represents a view of a two-dimensional area.
    /// </summary>
    public struct View
    {
        public View(Point Offset, Point Right, Point Down)
        {
            this.Offset = Offset;
            this.Right = Right;
            this.Down = Down;
        }

        public View(Rectangle ViewRectangle)
        {
            this.Offset = ViewRectangle.TopLeft;
            this.Right = new Point(ViewRectangle.Right - ViewRectangle.Left, 0.0);
            this.Down = new Point(0.0, ViewRectangle.Bottom - ViewRectangle.Top);
        }

        /// <summary>
        /// Gets the amount of area in world space this view covers.
        /// </summary>
        public double Area
        {
            get
            {
                return this.Right.X * this.Down.Y - this.Right.Y * this.Down.X;
            }
        }

        /// <summary>
        /// Projections a point from view space to world space.
        /// </summary>
        public Point Project(Point View)
        {
            return this.Offset + this.Right * View.X + this.Down * View.Y;
        }

        /// <summary>
        /// The location of the top-left corner of the view in world space.
        /// </summary>
        public Point Offset;

        /// <summary>
        /// The vector from the left edge of the the view to the corresponding point on the right edge of
        /// the view in world space.
        /// </summary>
        public Point Right;

        /// <summary>
        /// The vector from the top edge of the the view to the corresponding point on the bottom edge of
        /// the view in world space.
        /// </summary>
        public Point Down;
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
        /// Gets the view for this camera using a viewport with the given aspect ratio.
        /// </summary>
        public View GetView(double AspectRatio)
        {
            double size = this.Scale;
            Point off = AspectRatio > 1.0 ? new Point(size * AspectRatio, size) : new Point(size, size / AspectRatio);
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