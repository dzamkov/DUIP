using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.GUI
{
    /// <summary>
    /// A graphical, rectangular representation of a logical object.
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
        /// Gets the position of the topleft corner of the node.
        /// </summary>
        public Point Position
        {
            get
            {
                return this._Position;
            }
        }

        /// <summary>
        /// Gets the velocity of the node.
        /// </summary>
        public Point Velocity
        {
            get
            {
                return this._Velocity;
            }
        }

        /// <summary>
        /// Independently updates the state of this node.
        /// </summary>
        public void Update(double Time, double Damping)
        {
            this._Position += this._Velocity * Time;
            this._Velocity *= Math.Pow(Damping, Time);
        }

        private Point _Position;
        private Point _Velocity;
        private Point _Size;
    }
}