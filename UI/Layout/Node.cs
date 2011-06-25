﻿using System;
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
    public class Node
    {
        public Node(Disposable<Content> Content, Point Position, Point Velocity)
        {
            this._Content = Content;
            this._Position = Position;
            this._Velocity = Velocity;
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
                return ((Content)this._Content).Size;
            }
        }

        /// <summary>
        /// Gets the content for the node.
        /// </summary>
        public Content Content
        {
            get
            {
                return this._Content;
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
        /// Independently updates the state of this node.
        /// </summary>
        public void Update(World World, IEnumerable<Probe> Probes, double Time)
        {
            this._Position += this._Velocity * Time;
            this._Velocity *= Math.Pow(World.Damping, Time);
            this._Content = ((Content)this._Content).Update(this, Probes, Time);

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
                            this._DragState = new DragState
                            {
                                Probe = probe,
                                Offset = pos - this._Position
                            };
                            probe.Lock();
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
                            probe.Release();
                            this._DragState = null;
                        }
                        probefound = true;
                    }
                }
                if (!probefound)
                {
                    this._DragState = null;
                }
            }
        }

        /// <summary>
        /// Drags the point at the given offset on the node towards the given world position.
        /// </summary>
        public void Pull(Point Offset, Point Position, double Time)
        {
            const double velsmooth = 2.0;
            Point anchor = this._Position + Offset;
            Point dir = Position - anchor;
            this._Velocity = (dir / Time + this._Velocity * velsmooth) / (1.0 + velsmooth);
            this._Position = (Position - Offset);
        }

        /// <summary>
        /// Independently renders this node using the given context.
        /// </summary>
        public void Render(World World, RenderContext Context)
        {
            using (Context.Translate(this._Position))
            {
                ((Content)this._Content).Render(Context);
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

        private Point _Position;
        private Point _Velocity;
        private DragState _DragState;
        private Disposable<Content> _Content;
    }

    /// <summary>
    /// Content a node can display.
    /// </summary>
    public abstract class Content
    {
        /// <summary>
        /// Gets the size of the content when rendered.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Renders the content to the given render context.
        /// </summary>
        public virtual void Render(RenderContext Context)
        {

        }

        /// <summary>
        /// Updates the state of the content by the given amount of time while receiving input from probes. Returns the
        /// new state of the content. If the interface for the content changes, the older interface will be disposed.
        /// </summary>
        /// <param name="Node">The node this content is in.</param>
        /// <param name="Probes">The probes in the world.</param>
        public virtual Disposable<Content> Update(Node Node, IEnumerable<Probe> Probes, double Time)
        {
            return this;
        }
    }
}