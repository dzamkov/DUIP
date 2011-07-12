using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// A dynamic graphical, rectangular representation of a logical object.
    /// </summary>
    public class Node : IDisposable
    {
        public Node(Disposable<Content> Content, Disposable<Block> Block, Point Position, Point Velocity)
        {
            this._Content = Content;
            this._Block = Block;
            this._Position = Position;
            this._Velocity = Velocity;
            this._Layout = this._Block.Object.CreateLayout(null, SizeRange, out this._Size);
        }

        public Node(Disposable<Content> Content, Disposable<Block> Block, Layout Layout, Point Size, Point Position, Point Velocity)
        {
            this._Content = Content;
            this._Block = Block;
            this._Position = Position;
            this._Velocity = Velocity;
            this._Layout = Layout;
            this._Size = Size;
        }

        /// <summary>
        /// Gets the size range available for a node.
        /// </summary>
        public static Rectangle SizeRange = new Rectangle(0.1, 0.1, 5.0, 5.0);

        /// <summary>
        /// Gets the area this node covers.
        /// </summary>
        public Rectangle Area
        {
            get
            {
                return new Rectangle(this._Position, this._Position + this.Size);
            }
        }

        /// <summary>
        /// Gets the size of the node.
        /// </summary>
        public Point Size
        {
            get
            {
                return this._Size;
            }
        }

        /// <summary>
        /// Gets the content this node represents.
        /// </summary>
        public Content Content
        {
            get
            {
                return this._Content;
            }
        }

        /// <summary>
        /// Gets the block displayed by the node.
        /// </summary>
        public Block Block
        {
            get
            {
                return this._Block;
            }
        }

        /// <summary>
        /// Gets or sets the position of the topleft corner of the node.
        /// </summary>
        public Point Position
        {
            get
            {
                return this._Position;
            }
            set
            {
                this._Position = value;
            }
        }

        /// <summary>
        /// Gets or sets the velocity of the node.
        /// </summary>
        public Point Velocity
        {
            get
            {
                return this._Velocity;
            }
            set
            {
                this._Velocity = value;
            }
        }

        /// <summary>
        /// Handles a probe signal change over the node.
        /// </summary>
        /// <param name="Offset">The offset of the probe from the top-left corner of the node.</param>
        public void ProbeSignalChange(World World, Probe Probe, Point Offset, ProbeSignal Signal, bool Value)
        {
            // Start dragging if possible 
            if (this._DragState == null && Signal == ProbeSignal.Primary && Value == true)
            {
                this._DragState = new DragState
                {
                    Offset = Offset,
                    Probe = Probe,
                    ReleaseProbe = Probe.Lock()
                };
            }
        }

        /// <summary>
        /// Independently updates the state of this node.
        /// </summary>
        public void Update(World World, double Time)
        {
            this._Position += this._Velocity * Time;
            this._Velocity *= Math.Pow(World.Damping, Time);

            // Handle dragging
            DragState dragstate = this._DragState;
            if (dragstate != null)
            {
                Probe dragprobe = dragstate.Probe;
                if (dragprobe[ProbeSignal.Primary])
                {
                    this.Pull(dragstate.Offset, dragprobe.Position, Time);
                }
                else
                {
                    dragstate.ReleaseProbe();
                    this._DragState = null;
                }
            }
        }

        /// <summary>
        /// Drags the point at the given offset on the node towards the given world position.
        /// </summary>
        public void Pull(Point Offset, Point Position, double Time)
        {
            const double velsmooth = 4.0;
            const double possmooth = 1.0;
            Point tar = (Position - Offset);
            Point dif = tar - this._Position;
            this._Velocity = (dif / Time + this._Velocity * velsmooth) / (1.0 + velsmooth);
            this._Position = (tar + this._Position * possmooth) / (1.0 + possmooth);
        }

        /// <summary>
        /// Performs a collision response for two nodes.
        /// </summary>
        public static void CollisionResponse(Node A, Node B)
        {
            const double res = 0.9;
            const double fri = 0.1;
            const double pus = 0.4;
            Point asize = A.Size;
            Point bsize = B.Size;
            Point tsize = asize + bsize;

            Point acenter = A._Position + asize * 0.5;
            Point bcenter = B._Position + bsize * 0.5;

            Point diff = bcenter - acenter;
            Point pen = tsize * 0.5 - new Point(Math.Abs(diff.X), Math.Abs(diff.Y));
            Point vdiff = B._Velocity - A._Velocity;

            if (pen.X > 0.0 && pen.Y > 0.0)
            {
                if (pen.X < pen.Y)
                {
                    if (diff.X > 0.0)
                    {
                        A._Position -= new Point(pen.X * 0.5, 0.0);
                        B._Position += new Point(pen.X * 0.5, 0.0);
                    }
                    else
                    {
                        A._Position += new Point(pen.X * 0.5, 0.0);
                        B._Position -= new Point(pen.X * 0.5, 0.0);
                    }
                    A._Velocity += new Point(vdiff.X * res, vdiff.Y * fri - diff.Y * Math.Abs(vdiff.X) * pus);
                    B._Velocity -= new Point(vdiff.X * res, vdiff.Y * fri - diff.Y * Math.Abs(vdiff.X) * pus);
                }
                else
                {
                    if (diff.Y > 0.0)
                    {
                        A._Position -= new Point(0.0, pen.Y * 0.5);
                        B._Position += new Point(0.0, pen.Y * 0.5);
                    }
                    else
                    {
                        A._Position += new Point(0.0, pen.Y * 0.5);
                        B._Position -= new Point(0.0, pen.Y * 0.5);
                    }
                    A._Velocity += new Point(vdiff.X * fri - diff.X * Math.Abs(vdiff.Y) * pus, vdiff.Y * res);
                    B._Velocity -= new Point(vdiff.X * fri - diff.X * Math.Abs(vdiff.Y) * pus, vdiff.Y * res);
                }
            }
        }

        /// <summary>
        /// Independently renders this node using the given context.
        /// </summary>
        public void Render(World World, RenderContext Context)
        {
            using (Context.Translate(this._Position))
            {
                this._Layout.Render(Context);
            }
        }

        /// <summary>
        /// Contains information about the dragging state of a node, for when a node is being dragged by a probe.
        /// </summary>
        public class DragState
        {
            /// <summary>
            /// The probe that is dragging the node.
            /// </summary>
            public Probe Probe;

            /// <summary>
            /// Releases the lock on the probe for this drag state.
            /// </summary>
            public Action ReleaseProbe;

            /// <summary>
            /// The offset of the point that is being dragged towards the probe in relation to the node.
            /// </summary>
            public Point Offset;
        }

        public void Dispose()
        {
            this._Content.Dispose();
            this._Block.Dispose();
        }

        private Point _Position;
        private Point _Velocity;
        private DragState _DragState;
        private Disposable<Content> _Content;
        private Disposable<Block> _Block;
        private Layout _Layout;
        private Point _Size;
    }
}