using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

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
        /// Gets a bitmap representation of an area within the figure. The sampling method used is undefined.
        /// </summary>
        public virtual Bitmap GetArea(Rectangle Area, int Width, int Height)
        {
            Bitmap bm = new Bitmap(Width, Height);
            unsafe
            {
                BitmapData bmd = bm.LockBits(
                    new System.Drawing.Rectangle(0, 0, Width, Height),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                byte* data = (byte*)bmd.Scan0.ToPointer();
                Point size = Area.Size;
                Point delta = new Point(size.X / Width, size.Y / Height);
                Point start = delta * 0.5 + Area.TopLeft;

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Color col = this.GetPoint(start + new Point(x * delta.X, y * delta.Y));
                        data[3] = (byte)(col.A * 255.0);
                        data[2] = (byte)(col.R * 255.0);
                        data[1] = (byte)(col.G * 255.0);
                        data[0] = (byte)(col.B * 255.0);
                        data += 4;
                    }
                }

                bm.UnlockBits(bmd);
            }
            return bm;
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

        public override Bitmap GetArea(Rectangle Area, int Width, int Height)
        {
            Bitmap b = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(this._Color);
            }
            return b;
        }

        private Color _Color;
    }
}