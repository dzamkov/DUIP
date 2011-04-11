using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A two-dimensional unparameterized curve that can be either open or closed. When an object is set to follow or trace a path, the start point and direction
    /// are implicitly given by the path.
    /// </summary>
    public abstract class Path
    {
        /// <summary>
        /// Gets a rectanglular area such that the path does not contain any points outside the rectangle.
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Unbound;
            }
        }

        /// <summary>
        /// Creates a path along a line.
        /// </summary>
        public static LinePath Line(Point Start, Point End)
        {
            return new LinePath(Start, End);
        }
    }

    /// <summary>
    /// The path of a single straight line.
    /// </summary>
    public class LinePath : Path
    {
        public LinePath(Point Start, Point End)
        {
            this._Start = Start;
            this._End = End;
        }

        /// <summary>
        /// Gets the start point of the line.
        /// </summary>
        public Point Start
        {
            get
            {
                return this._Start;
            }
        }

        /// <summary>
        /// Gets the end point of the line.
        /// </summary>
        public Point End
        {
            get
            {
                return this._End;
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle(
                    Math.Min(this._Start.X, this._End.X),
                    Math.Min(this._Start.Y, this._End.Y),
                    Math.Max(this._Start.X, this._End.X),
                    Math.Max(this._Start.Y, this._End.Y));
            }
        }

        private Point _Start;
        private Point _End;
    }

    /// <summary>
    /// Describes a method of tracing a path with color.
    /// </summary>
    public abstract class StrokeStyle
    {
        /// <summary>
        /// Creates a solid stroke style.
        /// </summary>
        public static SolidStrokeStyle Solid(Color Color, double Thickness)
        {
            return new SolidStrokeStyle(Color, Thickness);
        }
    }

    /// <summary>
    /// A stroke style given with a solid color and a thickness.
    /// </summary>
    public class SolidStrokeStyle : StrokeStyle
    {
        public SolidStrokeStyle(Color Color, double Thickness)
        {
            this._Color = Color;
            this._Thickness = Thickness;
        }

        /// <summary>
        /// Gets the color of the stroke.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        /// <summary>
        /// Gets the thickness of the stroke.
        /// </summary>
        public double Thickness
        {
            get
            {
                return this._Thickness;
            }
        }

        private Color _Color;
        private double _Thickness;
    }
}