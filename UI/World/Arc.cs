using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// A physical connection between two nodes.
    /// </summary>
    public class Arc
    {
        public Arc(EndPoint Start, EndPoint End, double Thickness, Color Color)
        {
            this._Start = Start;
            this._End = End;
            this._Thickness = Thickness;
            this._Color = Color;
        }

        /// <summary>
        /// Gets the endpoint for the start of the arc.
        /// </summary>
        public EndPoint Start
        {
            get
            {
                return this._Start;
            }
        }

        /// <summary>
        /// Gets the endpoint for the end of the arc.
        /// </summary>
        public EndPoint End
        {
            get
            {
                return this._End;
            }
        }

        /// <summary>
        /// Represents an endpoint for an arc.
        /// </summary>
        public struct EndPoint
        {
            public EndPoint(Node Node, Direction Edge, double Offset)
            {
                this.Node = Node;
                this.Edge = Edge;
                this.Offset = Offset;
            }

            /// <summary>
            /// Gets the position of this endpoint.
            /// </summary>
            public Point Position
            {
                get
                {
                    Point npos = this.Node.Position;
                    Point nsize = this.Node.Size;
                    double off = this.Offset;
                    switch (this.Edge)
                    {
                        case Direction.Left: return new Point(npos.X, npos.Y + nsize.Y - off);
                        case Direction.Up: return new Point(npos.X + off, npos.Y);
                        case Direction.Right: return new Point(npos.X + nsize.X, npos.Y + off);
                        default: return new Point(npos.X + nsize.X - off, npos.Y + nsize.Y);
                    }
                }
            }

            /// <summary>
            /// Gets the normal (direction that faces outward from the node) of the endpoint.
            /// </summary>
            public Point Normal
            {
                get
                {
                    return Point.Unit(Edge);
                }
            }

            /// <summary>
            /// The node at this endpoint.
            /// </summary>
            public Node Node;

            /// <summary>
            /// The direction for the edge this endpoint is on.
            /// </summary>
            public Direction Edge;

            /// <summary>
            /// The offset of the endpoint from the beginning of the edge.
            /// </summary>
            public double Offset;
        }

        /// <summary>
        /// Renders this arc to the given context.
        /// </summary>
        public void Render(World World, RenderContext Context)
        {
            Context.ClearTexture();
            Context.SetColor(this._Color);

            
            Point a = this._Start.Position;
            Point d = this._End.Position;

            double proj = (a - d).Length * 0.3;
            Point b = a + this._Start.Normal * proj;
            Point c = d + this._End.Normal * proj;

            Context.DrawBezierCurve(a, b, c, d, this._Thickness);
        }

        /// <summary>
        /// Updates the state of the arc and influences the endpoint nodes as needed.
        /// </summary>
        public void Update(World World, double Time)
        {

        }

        private EndPoint _Start;
        private EndPoint _End;
        private double _Thickness;
        private Color _Color;
    }
}