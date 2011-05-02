using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// A description of a colored object or image (with alpha channel) on an infinitely large and precise
    /// two-dimensional plane.
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

        public override Color GetPoint(Point Point)
        {
            return this._Color;           
        }

        public override unsafe void GetArea(Rectangle Area, int Width, int Height, byte* Data)
        {
            byte a = (byte)(this._Color.A * 255.0);
            byte r = (byte)(this._Color.R * 255.0);
            byte g = (byte)(this._Color.G * 255.0);
            byte b = (byte)(this._Color.B * 255.0);

            int m = Width * Height;
            for (int t = 0; t < m; t++)
            {
                Data[3] = a;
                Data[2] = r;
                Data[1] = g;
                Data[0] = b;
            }
        }

        private Color _Color;
    }
}