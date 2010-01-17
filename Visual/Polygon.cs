//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using DUIP.Core;

namespace DUIP.Visual
{
    /// <summary>
    /// The type of line or curve defined by the point.
    /// </summary>
    public enum PolygonPointFlags
    {
        /// <summary>
        /// A point of this kind will create a linear connection to
        /// the next point.
        /// </summary>
        Linear = 0x00000001,

        /// <summary>
        /// A point of this kind marks the start of a new bezier segment.
        /// </summary>
        BezierStart = 0x00000002,

        /// <summary>
        /// This point acts as a guide on a bezier segment already started.
        /// </summary>
        BezierControl = 0x00000004,

        /// <summary>
        /// This point's connection to the next is not drawn.
        /// </summary>
        Hidden = 0x00000008,

        /// <summary>
        /// If this point has NoFill'd neighbohrs on both sides of it, the
        /// triangle between them is not given in the triangle set.
        /// </summary>
        NoFill = 0x00000010,

        /// <summary>
        /// This point is at the end of a polygon.
        /// </summary>
        End = 0x00000100,
    }

    /// <summary>
    /// A point on a polygon. A points options define the shape and properties
    /// of the next connection in the polygon.
    /// </summary>
    public struct PolygonPoint
    {
        public PolygonPoint(Point Point)
        {
            this.Point = Point;
            this.Type = PolygonPointFlags.Linear;
        }

        public PolygonPoint(Point Point, PolygonPointFlags Type)
        {
            this.Point = Point;
            this.Type = Type;
        }

        /// <summary>
        /// Location of the point.
        /// </summary>
        public Point Point;

        /// <summary>
        /// The type of this point.
        /// </summary>
        public PolygonPointFlags Type;
    }

    /// <summary>
    /// Repesentation of a polygon that can be concave, curved, open or closed. Polygons
    /// are drawn by pens and brushes.
    /// </summary>
    public abstract class Polygon
    {

        /// <summary>
        /// Gets the list of complex points that make up this polygon.
        /// </summary>
        public abstract IEnumerable<PolygonPoint> Points { get; }

        /// <summary>
        /// Gets if the polygon is closed. This is true if the last point in the
        /// polygon is not of type End.
        /// </summary>
        public abstract bool Closed { get; }

        /// <summary>
        /// Gets an enumerable list of simple points for this polygon.
        /// </summary>
        public IEnumerable<Point> PointList
        {
            get
            {
                foreach(PolygonPoint pp in this.Points)
                {
                    yield return pp.Point;
                }
            }
        }

        /// <summary>
        /// Creates a polygon representation of this made entirely of lines.
        /// </summary>
        /// <param name="Resolution">The amount to subdivide curves by. A higher
        /// value indicates higher level subdivison. For a circle that occupies the
        /// entire screen, this should be 100. A value of 0 converts curves to lines.</param>
        /// <returns></returns>
        public virtual Polygon Simplify(double Resolution)
        {
            ComplexPolygon simp = new ComplexPolygon();
            foreach (PolygonPoint pp in this.Points)
            {
                // Convert everything directly to a line for now.
                simp.PushPoint(new PolygonPoint(pp.Point, pp.Type & (PolygonPointFlags.End | PolygonPointFlags.Linear)));
            }
            return simp;
        }

        /// <summary>
        /// Creates a list of triangles that represent the filled area in the polygon. This only works 
        /// if the polygon is closed and contains no complex points.
        /// </summary>
        /// <returns>The filled area of this polygon represented with triangles.</returns>
        public virtual IEnumerable<Triangle> TriangularDecomposition()
        {
            // Initialize point list
            LinkedList<PolygonPoint> points = new LinkedList<PolygonPoint>();
            foreach (PolygonPoint pp in this.Points)
            {
                points.AddLast(pp);
                if ((pp.Type & PolygonPointFlags.Linear) == 0 || (pp.Type & PolygonPointFlags.End) > 0)
                {
                    throw new Exception("Can't decompose this triangle, sorry");
                }
            }

            LinkedListNode<PolygonPoint> current = points.First;
            LinkedListNode<PolygonPoint> lasttri = current;
            while (true)
            {
                // Get next and previous points around current.
                LinkedListNode<PolygonPoint> next = current.Next;
                LinkedListNode<PolygonPoint> prev = current.Previous;
                if (next == null)
                {
                    next = points.First;
                }
                if (prev == null)
                {
                    prev = points.Last;
                }
                if (next == prev)
                {
                    break;
                }

                // Create and test triangle
                Triangle tri = new Triangle(prev.Value.Point, current.Value.Point, next.Value.Point);
                if (tri.Valid)
                {
                    // Add only if filled
                    if ((next.Value.Type & prev.Value.Type & current.Value.Type & PolygonPointFlags.NoFill) == 0)
                    {
                        yield return tri;
                    }

                    // Remove point and continue
                    prev = current;
                    current = next;
                    lasttri = current;
                    points.Remove(prev);
                }
                else
                {
                    // Continue
                    current = next;

                    // Check if whole polygon was traversed.
                    if (current == lasttri)
                    {
                        throw new Exception("Invalide polygon");
                    }
                }
            }
        }
    }

    /// <summary>
    /// A triangular polygon.
    /// </summary>
    public sealed class Triangle : Polygon
    {
        public Triangle()
        {
            this._Points = new Point[3];
        }

        public Triangle(Point A, Point B, Point C)
        {
            this._Points = new Point[] { A, B, C };
        }

        public override IEnumerable<PolygonPoint> Points
        {
            get
            {
                PolygonPoint[] plist = new PolygonPoint[3];
                for (int t = 0; t < 3; t++)
                {
                    plist[t] = new PolygonPoint(this._Points[t], PolygonPointFlags.Linear);
                }
                return plist;
            }
        }

        public override bool Closed
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets if this is a valid clockwise triangle.
        /// </summary>
        public bool Valid
        {
            get
            {
                SVector a = this._Points[0]._Vector;
                SVector b = this._Points[1]._Vector;
                SVector c = this._Points[2]._Vector;
                SVector ba = b - a;
                SVector ca = c - a;
                if (ba.Right * ca.Down > ba.Down * ca.Right)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        Point[] _Points;
    }

    /// <summary>
    /// A polygon that implements all types of points and features of polygon.
    /// </summary>
    public class ComplexPolygon : Polygon
    {
        public ComplexPolygon()
        {
            this._Points = new LinkedList<PolygonPoint>();
        }

        public override IEnumerable<PolygonPoint> Points
        {
            get
            {
                return this._Points;
            }
        }

        public override bool Closed
        {
            get
            {
                return (this._Points.Last.Value.Type & PolygonPointFlags.End) == 0;
            }
        }

        /// <summary>
        /// Appends a point to this polygon. Remeber that polygon's points are
        /// defined in clockwise order.
        /// </summary>
        /// <param name="Point">The point to add.</param>
        public void PushPoint(PolygonPoint Point)
        {
            this._Points.AddLast(Point);
        }

        /// <summary>
        /// Appends a point as a line segment.
        /// </summary>
        /// <param name="Point">The point to add.</param>
        public void PushPoint(Point Point)
        {
            this.PushPoint(new PolygonPoint(Point));
        }

        LinkedList<PolygonPoint> _Points;
    }
}
