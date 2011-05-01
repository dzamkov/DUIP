using System;
using System.Collections.Generic;

namespace DUIP.GUI
{
    /// <summary>
    /// A description of a colored object or image (with alpha channel) on an infinitely large and precise
    /// two-dimensional plane.
    /// </summary>
    public abstract class Figure
    {
        /// <summary>
        /// Gets the color of a point on the figure.
        /// </summary>
        public virtual Color GetPoint(Point Point)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the color data of an area on the figure sampled to a finite resolution in bgra format to
        /// a pointer. The sampling method used is undefined.
        /// </summary>
        public unsafe virtual void GetArea(Rectangle Area, int Width, int Height, byte* Data)
        {
            Point size = Area.Size;
            Point delta = new Point(size.X / Width, size.Y / Height);
            Point start = delta * 0.5 + Area.TopLeft;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Color col = this.GetPoint(start + new Point(x * delta.X, y * delta.Y));
                    Data[3] = (byte)(col.A * 255.0);
                    Data[2] = (byte)(col.R * 255.0);
                    Data[1] = (byte)(col.G * 255.0);
                    Data[0] = (byte)(col.B * 255.0);
                    Data += 4;
                }
            }
        }

        /// <summary>
        /// Renders the figure to the current graphics context when the given view is used.
        /// </summary>
        public virtual void Render(View View)
        {
            throw new NotImplementedException();
        }

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