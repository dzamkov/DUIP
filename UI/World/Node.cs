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
        public Node(Disposable<Content> Content, Disposable<Visual> Visual, Point Position, Point Velocity)
        {
            this._Content = Content;
            this._Visual = Visual;
            this._Position = Position;
            this._Velocity = Velocity;
            this._Texture = Texture.Create(this.Visual.Render, new View(Rectangle.FromOffsetSize(Point.Origin, this.Visual.Size)), Texture.Format.BGRA32, 256, 256);

            this._Texture.Bind();
            Texture.GenerateMipmap();
            Texture.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
        }

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
                return ((Visual)this._Visual).Size;
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
        /// Gets or sets the visual displayed by the node.
        /// </summary>
        public Visual Visual
        {
            get
            {
                return this._Visual;
            }
            set
            {
                this._Visual = value;
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
        /// Gets the node being dragged by the given probe, or null if the probe is not dragging a node.
        /// </summary>
        public static Node Dragged(Probe Probe)
        {
            Node node = Probe.Lock as Node;
            if (node._DragState != null && node._DragState.Probe == Probe)
            {
                return node;
            }
            return null;
        }

        /// <summary>
        /// Sets the probe that is dragging this node, along with the target offset of the probe from the node. The probe will
        /// need to be active to maintain dragging.
        /// </summary>
        public void SetDrag(Probe Probe, Point Offset)
        {
            if (this._DragState != null)
            {
                this._DragState.Probe.Lock = null;
            }
            this._DragState = new DragState
            {
                Probe = Probe,
                Offset = Offset
            };
            Probe.Lock = this;
        }

        /// <summary>
        /// Insures that the node is not being dragged by any probes.
        /// </summary>
        public void CancelDrag()
        {
            if (this._DragState != null)
            {
                this._DragState.Probe.Lock = null;
                this._DragState = null;
            }
        }

        /// <summary>
        /// Independently updates the state of this node.
        /// </summary>
        public void Update(World World, IEnumerable<Probe> Probes, double Time)
        {
            this._Position += this._Velocity * Time;
            this._Velocity *= Math.Pow(World.Damping, Time);
            this._Visual = ((Visual)this._Visual).Update(this, Probes, Time);

            // Handle dragging
            if (this._DragState == null)
            {
                foreach (Probe probe in Probes)
                {
                    Point pos = probe.Position;
                    if (this.Area.Occupies(pos))
                    {
                        if (probe.Use(this) && probe.Active)
                        {
                            this.SetDrag(probe, pos - this._Position);
                        }
                    }
                }
            }
            else
            {
                bool probefound = false;
                foreach (Probe probe in Probes)
                {
                    if (probe == this._DragState.Probe)
                    {
                        if (probe.Active)
                        {
                            this.Pull(this._DragState.Offset, probe.Position, Time);
                        }
                        else
                        {
                            this.CancelDrag();
                        }
                        probefound = true;
                        break;
                    }
                }
                if (!probefound)
                {
                    this.CancelDrag();
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
                double zoom = Context.View.Zoom;
                if (zoom > 1.0)
                {
                    if (zoom < 4.0)
                    {
                        double alpha = (zoom - 1.0) / (4.0 - 1.0);
                        ((Visual)this._Visual).Render(Context);
                        this._Texture.CreateFigure(new Rectangle(Point.Origin, this.Size), Color.RGBA(1.0, 1.0, 1.0, alpha)).Render(Context);
                    }
                    else
                    {
                        this._Texture.CreateFigure(new Rectangle(Point.Origin, this.Size)).Render(Context);
                    }
                }
                else
                {
                    ((Visual)this._Visual).Render(Context);
                }
            }
        }

        /// <summary>
        /// Contains information about the dragging state of a node, for when a node is being dragged by a probe.
        /// </summary>
        public class DragState
        {
            /// <summary>
            /// Gets the probe that is dragging the node.
            /// </summary>
            public Probe Probe;

            /// <summary>
            /// Gets the offset of the point that is being dragged towards the probe in relation to the node.
            /// </summary>
            public Point Offset;
        }

        public void Dispose()
        {
            this._Content.Dispose();
            this._Visual.Dispose();
        }

        private Texture _Texture;
        private Point _Position;
        private Point _Velocity;
        private DragState _DragState;
        private Disposable<Content> _Content;
        private Disposable<Visual> _Visual;
    }

    /// <summary>
    /// A visual representation of content that can be placed within a node.
    /// </summary>
    public abstract class Visual
    {
        /// <summary>
        /// Gets the size of the visual for rendering and layout.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Updates the state of the visual by the given amount of time while receiving input from probes. Returns an interface to the
        /// new state of the visual. If the interface for the visual changes, the older interface will be disposed.
        /// </summary>
        /// <param name="Node">The node this visual is in.</param>
        /// <param name="Probes">The probes in the world.</param>
        public virtual Disposable<Visual> Update(Node Node, IEnumerable<Probe> Probes, double Time)
        {
            return this;
        }

        /// <summary>
        /// Renders this visual to the given context.
        /// </summary>
        public virtual void Render(RenderContext Context)
        {

        }
    }
}