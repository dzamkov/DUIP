using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// An infinitely large, dynamic two-dimensional plane containing nodes and other graphical objects.
    /// </summary>
    public class World
    {
        public World()
        {
            this._Nodes = new List<Node>();
        }

        /// <summary>
        /// Gets the velocity damping of nodes and other objects within the world. This is expressed as the proportion of velocity
        /// an object retains every time unit.
        /// </summary>
        public double Damping
        {
            get
            {
                return 0.5;
            }
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
        /// Places the given node into the world, displacing other nodes as needed.
        /// </summary>
        public void Spawn(Node Node)
        {
            this._Nodes.Add(Node);
        }

        /// <summary>
        /// Updates the state of the world by the given amount of time.
        /// </summary>
        public void Update(IEnumerable<Probe> Probes, double Time)
        {
            foreach (Node n in this._Nodes)
            {
                n.Update(this, Probes, Time);
            }
        }

        /// <summary>
        /// Renders the world using the given context.
        /// </summary>
        public void Render(RenderContext Context)
        {
            foreach (Node n in this._Nodes)
            {
                n.Render(this, Context);
            }
        }

        private List<Node> _Nodes;
    }
}