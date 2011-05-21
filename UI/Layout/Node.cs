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
        public Node(Content Content, Point Position, Point Velocity)
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
                return this._Content.Size;
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
            this._Content.Update(this._Position, Probes, Time);
        }

        /// <summary>
        /// Independently renders this node using the given context.
        /// </summary>
        public void Render(World World, RenderContext Context)
        {
            using (Context.Translate(this._Position))
            {
                this._Content.Render(Context);
            }
        }

        private Point _Position;
        private Point _Velocity;
        private Content _Content;
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
        /// Updates the state of the content by the given amount of time while receiving input from probes.
        /// </summary>
        /// <param name="Offset">The offset of the control in relation to the world.</param>
        /// <param name="Probes">The probes in the world.</param>
        public virtual void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {

        }
    }
}