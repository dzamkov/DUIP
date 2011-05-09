﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using OpenTK;

namespace DUIP.UI
{
    /// <summary>
    /// A two-dimensional floating point position or offset (vector).
    /// </summary>
    public struct Point
    {
        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Gets the point at the origin of the coordinate space.
        /// </summary>
        public static readonly Point Origin = new Point(0.0, 0.0);

        /// <summary>
        /// Gets an offset that does not change the value of a point when
        /// added or subtracted to it.
        /// </summary>
        public static readonly Point Zero = Origin;

        /// <summary>
        /// Gets the square of the length of this point offset (vector). This function is quicker to compute than the actual length
        /// because it avoids a square root, which may be costly.
        /// </summary>
        public double SquareLength
        {
            get
            {
                return this.X * this.X + this.Y * this.Y;
            }
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(this.SquareLength);
            }
        }

        /// <summary>
        /// The width divided by the height of the size represented by this point.
        /// </summary>
        public double AspectRatio
        {
            get
            {
                return this.X / this.Y;
            }
        }

        /// <summary>
        /// Gets an offset perpendicular to this one counterclockwise.
        /// </summary>
        public Point Perpendicular
        {
            get
            {
                return new Point(-this.Y, this.X);
            }
        }

        /// <summary>
        /// Creates a unit vector (point offset) for the specified angle.
        /// </summary>
        public static Point Unit(double Angle)
        {
            return new Point(Math.Sin(Angle), Math.Cos(Angle));
        }

        /// <summary>
        /// Scales the point by the given point.
        /// </summary>
        public Point Scale(Point Scale)
        {
            return new Point(this.X * Scale.X, this.Y * Scale.Y);
        }

        /// <summary>
        /// Gets a point that has its components swapped from this point.
        /// </summary>
        public Point Swap
        {
            get
            {
                return new Point(this.Y, this.X);
            }
        }

        /// <summary>
        /// Rounds the point to the nearest integer components.
        /// </summary>
        public Point Round
        {
            get
            {
                return new Point(Math.Round(this.X), Math.Round(this.Y));
            }
        }

        /// <summary>
        /// Rounds the point to the next highest integer components. Useful for sizes.
        /// </summary>
        public Point Ceiling
        {
            get
            {
                return new Point(Math.Ceiling(this.X), Math.Ceiling(this.Y));
            }
        }

        /// <summary>
        /// Gets the angle of this point (representing an offset).
        /// </summary>
        public double Angle
        {
            get
            {
                return Math.Atan2(this.Y, this.X);
            }
        }

        /// <summary>
        /// Gets the dot product of two points (representing offsets).
        /// </summary>
        public static double Dot(Point A, Point B)
        {
            return A.X * B.X + A.Y * B.Y;
        }

        public static implicit operator PointF(Point Point)
        {
            return new PointF((float)Point.X, (float)Point.Y);
        }

        public static implicit operator Vector2d(Point Point)
        {
            return new Vector2d(Point.X, Point.Y);
        }

        public static implicit operator Vector2(Point Point)
        {
            return new Vector2((float)Point.X, (float)Point.Y);
        }

        public static implicit operator Vector3d(Point Point)
        {
            return new Vector3d(Point.X, Point.Y, 0.0);
        }

        public static implicit operator Vector3(Point Point)
        {
            return new Vector3((float)Point.X, (float)Point.Y, 0.0f);
        }

        public static Point operator -(Point A, Point B)
        {
            return new Point(A.X - B.X, A.Y - B.Y);
        }

        public static Point operator -(Point A)
        {
            return new Point(-A.X, -A.Y);
        }

        public static Point operator +(Point A, Point B)
        {
            return new Point(A.X + B.X, A.Y + B.Y);
        }

        public static Point operator *(Point A, double B)
        {
            return new Point(A.X * B, A.Y * B);
        }

        public static bool operator ==(Point A, Point B)
        {
            return A.X == B.X && A.Y == B.Y;
        }

        public static bool operator !=(Point A, Point B)
        {
            return A.X != B.X || A.Y != B.Y;
        }

        public override bool Equals(object obj)
        {
            Point? p = obj as Point?;
            if (obj != null)
            {
                return p.Value == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ ~this.Y.GetHashCode();
        }

        public double X;
        public double Y;
    }
}