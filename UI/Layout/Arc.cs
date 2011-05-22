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
        private Arc()
        {

        }

        /// <summary>
        /// Creates an arc with the given parameters. Automatically adds joints between the two endpoints. 
        /// </summary>
        /// <param name="Joints">The amount of joints to add in the arc.</param>
        public static Arc Create(EndPoint Start, EndPoint End, double Thickness, Color Color, double Elasticity, double Rigidness, int NumJoints)
        {
            Point spos = Start.Position;
            Point epos = End.Position;
            Point dif = epos - spos;
            double dis = dif.Length;

            double jointspacing = dis / (NumJoints + 1);
            Point jointdelta = dif / (NumJoints + 1);

            Joint[] joints = new Joint[NumJoints];
            for (int t = 0; t < joints.Length; t++)
            {
                joints[t] = new Joint(spos + jointdelta * (t + 1), Point.Zero);
            }

            return new Arc
            {
                _Start = Start,
                _End = End,
                _Color = Color,
                _Thickness = Thickness,
                _Elasticity = Elasticity,
                _Rigidness = Rigidness,

                _Joints = joints,
                _JointSpacing = jointspacing
            };
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
        /// Information about a joint within an arc.
        /// </summary>
        public struct Joint
        {
            public Joint(Point Position, Point Velocity)
            {
                this.Position = Position;
                this.Velocity = Velocity;
            }

            /// <summary>
            /// The position of the joint.
            /// </summary>
            public Point Position;

            /// <summary>
            /// The velocity of the joint.
            /// </summary>
            public Point Velocity;
        }

        /// <summary>
        /// Renders this arc to the given context.
        /// </summary>
        public void Render(World World, RenderContext Context)
        {
            Context.ClearTexture();
            Context.SetColor(this._Color);
            using (Context.DrawLines(this._Thickness))
            {
                Point prev = this._Start.Position;
                foreach (Joint j in this._Joints)
                {
                    Point cur = j.Position;
                    Context.OutputLine(prev, cur);
                    prev = cur;
                }
                Context.OutputLine(prev, this._End.Position);
            }
        }

        /// <summary>
        /// Updates the state of the arc and influences the endpoint nodes as needed.
        /// </summary>
        public void Update(World World, double Time)
        {
            double damping = World.Damping;

            Point startpos = this._Start.Position;
            Point endpos = this._End.Position;
            Point startvel = Point.Zero;
            Point endvel = Point.Zero;
            Point startdir = this._Start.Normal;
            Point enddir = this._End.Normal;

            // Update spans
            int joints = this._Joints.Length;
            int spans = joints + 1;
            Point[] spandiffs = new Point[spans];
            double[] spanlens = new double[spans];
            
            Point prev = startpos;
            for(int t = 0; t < this._Joints.Length; t++)
            {
                Point cur = this._Joints[t].Position;
                spandiffs[t] = cur - prev;
                prev = cur;
            }
            spandiffs[joints] = endpos - prev;
            for (int t = 0; t < spans; t++)
            {
                spanlens[t] = spandiffs[t].Length;
            }

            if (joints == 0)
            {
                this._UpdateSpan(ref spandiffs[0], ref spanlens[0], startdir, enddir, ref startvel, ref endvel, Time);
            }
            else
            {
                this._UpdateSpan(ref spandiffs[0], ref spanlens[0], 
                    startdir, spandiffs[1] / -spanlens[1], 
                    ref startvel, ref this._Joints[0].Velocity, Time);
                for (int t = 1; t < joints; t++)
                {
                    this._UpdateSpan(ref spandiffs[t], ref spanlens[t],
                        spandiffs[t - 1] / spanlens[t - 1], spandiffs[t + 1] / -spanlens[t + 1],
                        ref this._Joints[t - 1].Velocity, ref this._Joints[t].Velocity, Time); 
                }
                this._UpdateSpan(ref spandiffs[joints], ref spanlens[joints], 
                    spandiffs[joints - 1] / spanlens[joints - 1], enddir, 
                    ref this._Joints[joints - 1].Velocity, ref endvel, Time);
            }

            // Update joints
            double dampingfortime = Math.Pow(damping, Time);
            for (int t = 0; t < this._Joints.Length; t++)
            {
                this._UpdateJoint(ref this._Joints[t], dampingfortime, Time);
            }
        }

        /// <summary>
        /// Updates a span between two joints.
        /// </summary>
        private void _UpdateSpan(ref Point Difference, ref double Length, Point ADirection, Point BDirection, ref Point AVelocity, ref Point BVelocity, double Time)
        {
            // Elasticity
            Point dir = Difference / Length;
            double elforce = ((Length / this._JointSpacing) - (this._JointSpacing / Length)) * Time * 100.0 / this._Elasticity;
            AVelocity += dir * elforce;
            BVelocity -= dir * elforce;
            
            // Rigidness
            double rforce = Time * this._Rigidness * 100.0;
            AVelocity += (BDirection + Difference / Length) * rforce;
            BVelocity += (ADirection - Difference / Length) * rforce;
        }

        /// <summary>
        /// Updates the state of a joint.
        /// </summary>
        private void _UpdateJoint(ref Joint Joint, double DampingForTime, double Time)
        {
            Joint.Velocity *= DampingForTime;
            Joint.Position += Joint.Velocity * Time;
        }

        private EndPoint _Start;
        private EndPoint _End;
        private Joint[] _Joints;
        private double _JointSpacing;
        
        private double _Elasticity;
        private double _Rigidness;
        private double _Thickness;
        private Color _Color;
    }
}