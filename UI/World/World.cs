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
        public World(Theme Theme)
        {
            this._Arcs = new List<Arc>();
            this._Nodes = new List<Node>();
            this._Theme = Theme;
        }

        /// <summary>
        /// Gets the visual theme for the world.
        /// </summary>
        public Theme Theme
        {
            get
            {
                return this._Theme;
            }
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
        /// Spawns a node for the given content near or at the given location.
        /// </summary>
        public Node Spawn(Disposable<Content> Content, Point Location)
        {
            Point size;
            Control control = Content.Object.CreateControl(this._Theme);
            Control.Layout layout = control.CreateLayout(Node.SizeRange, out size);
            Node node = new Node(Content, control, layout, size, Location - size * 0.5, Point.Zero);
            this.Spawn(node);
            return node;
        }

        /// <summary>
        /// Despawns a node, removing it from the world.
        /// </summary>
        public void Despawn(Node Node)
        {
            this._Nodes.Remove(Node);
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
        /// Gets the node at the given point, or null if no node occupies the point.
        /// </summary>
        public Node NodeAtPoint(Point Point)
        {
            foreach (Node node in this._Nodes)
            {
                if (node.Area.Occupies(Point))
                {
                    return node;
                }
            }
            return null;
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
        private Theme _Theme;
    }
}