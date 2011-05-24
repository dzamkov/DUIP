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
            this._Arcs = new List<Arc>();
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
                return 0.2;
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
        /// Places the given arc into the world, making it visible and allowing it to affect its nodes.
        /// </summary>
        public void Spawn(Arc Arc)
        {
            this._Arcs.Add(Arc);
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

            foreach (Arc a in this._Arcs)
            {
                a.Update(this, Time);
            }

            foreach (Node n in this._Nodes)
            {
                foreach (Node np in this._Nodes)
                {
                    if (n != np && n.GetHashCode() > np.GetHashCode() && Rectangle.Intersects(n.Area, np.Area))
                    {
                        Node.CollisionResponse(n, np);
                    }
                }
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
            foreach (Arc a in this._Arcs)
            {
                a.Render(this, Context);
            }
        }

        private List<Arc> _Arcs;
        private List<Node> _Nodes;
    }
}