using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// An infinitely large, dynamic two-dimensional plane containing nodes and other graphical objects.
    /// </summary>
    public class World
    {
        public World()
        {
            this._Nodes = new List<Node>();
            this._Nodes.Add(new Node(new Point(1.0, 1.0), new Point(0.0, 0.0), new Point(2.0, 0.0)));
        }

        /// <summary>
        /// Gets all the nodes currently in the world.
        /// </summary>
        public IEnumerable<Node> Nodes
        {
            get
            {
                return this._Nodes;
            }
        }

        /// <summary>
        /// Updates the state of the world by the given amount of time.
        /// </summary>
        public void Update(IEnumerable<Probe> Probes, double Time)
        {
            double damping = 0.5;
            foreach (Node n in this._Nodes)
            {
                n.Update(this, Probes, Time, damping);
            }
        }

        /// <summary>
        /// Renders the world to the current graphics context when the given view is used.
        /// </summary>
        public void Render(View View)
        {
            foreach (Node n in this._Nodes)
            {
                n.Render(this, View);
            }
        }

        private List<Node> _Nodes;
    }
}