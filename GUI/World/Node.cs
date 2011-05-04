using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// A dynamic graphical, rectangular representation of a logical object.
    /// </summary>
    public class Node
    {
        public Node(Point Size, Point Position, Point Velocity)
        {
            this._Size = Size;
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
                return new Rectangle(this._Position, this._Position + this._Size);
            }
        }

        /// <summary>
        /// Gets or sets the size of the node.
        /// </summary>
        public Point Size
        {
            get
            {
                return this._Size;
            }
            set
            {
                this._Size = value;
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
        public void Update(World World, IEnumerable<Probe> Probes, double Time, double Damping)
        {
            this._Position += this._Velocity * Time;
            this._Velocity *= Math.Pow(Damping, Time);

            foreach (Probe p in Probes)
            {
                if (this.Area.Occupies(p.Position) && p.Pressed)
                {
                    this._Velocity += new Point(1.0, 1.0) * Time;
                }
            }
        }

        /// <summary>
        /// Independently renders this node to the current graphics context.
        /// </summary>
        public void Render(World World, View View)
        {
            GL.Color3(Color.White);
            GL.Disable(EnableCap.Texture2D);
            Texture.DrawQuad(this.Area);
            GL.Enable(EnableCap.Texture2D);
        }

        private Point _Position;
        private Point _Velocity;
        private Point _Size;
    }
}