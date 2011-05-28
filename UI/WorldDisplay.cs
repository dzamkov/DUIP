using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using WPoint = System.Drawing.Point;

namespace DUIP.UI
{
    /// <summary>
    /// A control that allows the user to interact with a world.
    /// </summary>
    public class WorldDisplay : GLControl
    {
        public WorldDisplay()
            : base(GraphicsMode.Default)
        {
            this.AllowDrop = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.MakeCurrent();

            RenderContext.Initialize();
            this._Camera = new Camera(new Point(0.0, 0.0), 1.0);
            this._Background = new OceanAmbience(new Random());
            this._World = new World(new Theme());
            this._Probe = new Probe();
            this._MakeView();

            Content testcontent = new StaticContent<string>("Hello world", Type.String);
            this._World.Spawn(testcontent, Point.Origin);
        }

        /// <summary>
        /// Performs rendering on the view.
        /// </summary>
        public void Render()
        {
            this.MakeCurrent();
            RenderContext rc = new RenderContext(this._View, this.Width, this.Height, true);
            this._Background.Render(this._World, rc);
            this._World.Render(rc);
            this.SwapBuffers();

            ErrorCode ec = GL.GetError();
        }

        /// <summary>
        /// Updates the contents of the view by the given amount of time.
        /// </summary>
        public void Update(double Time)
        {
            double zrate = Time * 2.0;

            this._Camera.ZoomTo(this._Probe.Position, Time * -this._WheelDelta);
            this._Camera.Update(Time, 0.01, -2.0, 8.0);
            this._MakeView();
            this._WheelDelta = 0;

            Probe[] probes;
            if(this._HasProbe)
            {
                probes = new Probe[] { this._Probe };
                this._Probe.Prepare();
            }
            else
            {
                probes = new Probe[0];
            }
            this._World.Update(probes, Time);
            this._Background.Update(this._World, Time);  
        }

        protected override void OnResize(EventArgs e)
        {
            this._MakeView();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this._HasProbe = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this._HasProbe = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            WPoint loc = e.Location;
            this._Probe.UpdatePosition(this._Project(loc));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this._Probe.UpdateActive(true);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this._Probe.UpdateActive(false);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                WPoint point = new WPoint(e.X, e.Y);
                Point pos = this._Project(point);
                this._Probe.UpdatePosition(pos);
                ContextMenuStrip cms = this.BuildContextMenu(pos);
                if (cms != null)
                {
                    cms.Show(this, point);
                }
            }
        }

        /// <summary>
        /// Creates a context menu for interaction with the given world position, or returns null if no context menu is to be shown.
        /// </summary>
        public ContextMenuStrip BuildContextMenu(Point Point)
        {
            ContextMenuStrip cms = new ContextMenuStrip();

            Node node = this._World.NodeAtPoint(Point);
            if (node != null)
            {
                cms.Items.Add("Hide").Click += delegate
                {
                    this._World.Despawn(node);
                };

                Transfer.Item item = Content.Export(node.Content);
                if (item != null)
                {
                    cms.Items.Add("Copy").Click += delegate
                    {
                        item.SetClipboard();
                        return;
                    };
                }
            }


            Disposable<Content> curclipboard = Content.Import(Transfer.Item.FromClipboard());
            if (!curclipboard.IsNull)
            {
                bool selected = false;
                cms.Items.Add("Paste").Click += delegate
                {
                    selected = true;
                    this._World.Spawn(curclipboard, Point);
                };
                cms.Closed += delegate
                {
                    if (!selected)
                    {
                        curclipboard.Dispose();
                    }
                };
            }

            return cms;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this._WheelDelta += e.Delta;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            this._DragContent = Content.Import(Transfer.Item.FromDataObject(e.Data));
            if (this._DragContent.IsNull)
            {
                e.Effect = DragDropEffects.None;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            if (!this._DragContent.IsNull)
            {
                this.TopLevelControl.Focus();
                this._World.Spawn(this._DragContent, this._Project(this.PointToClient(new WPoint(e.X, e.Y))));
                this._DragContent = null;
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            if (!this._DragContent.IsNull)
            {
                this._DragContent.Dispose();
                this._DragContent = null;
            }
        }

        /// <summary>
        /// The probe used for a world view.
        /// </summary>
        public class Probe : DUIP.UI.Probe
        {
            /// <summary>
            /// Sets if this probe is active.
            /// </summary>
            public void UpdateActive(bool Active)
            {
                this._Active = Active;
            }

            /// <summary>
            /// Sets the position of this probe.
            /// </summary>
            public void UpdatePosition(Point Position)
            {
                this._Position = Position;
            }

            /// <summary>
            /// Prepares the probe to be used by a world.
            /// </summary>
            public void Prepare()
            {
                this._User = null;
            }

            public override bool Active
            {
                get
                {
                    return this._Active;
                }
            }

            public override object Lock
            {
                get
                {
                    return this._Lock;
                }
                set
                {
                    this._Lock = value;
                }
            }

            public override bool Use(object Object)
            {
                if (this._User == null)
                {
                    if (this._Lock == null || this._Lock == Object)
                    {
                        this._User = Object;
                        return true;
                    }
                }
                return false;
            }

            public override Point Position
            {
                get
                {
                    return this._Position;
                }
            }

            private Point _Position;
            private bool _Active;
            private object _User;
            private object _Lock;
        }

        /// <summary>
        /// Updates the current view for the camera state.
        /// </summary>
        private void _MakeView()
        {
            this._View = this._Camera.GetView(this.Width, this.Height);
        }

        /// <summary>
        /// Gets the world position of a point in coordinates relative to the client area of this control.
        /// </summary>
        private Point _Project(WPoint Point)
        {
            return this._View.Project(new Point(Point.X / (double)this.Width, Point.Y / (double)this.Height));
        }

        private int _WheelDelta;

        private Disposable<Content> _DragContent;
        private Probe _Probe;
        private bool _HasProbe;

        private Ambience _Background;
        private World _World;
        private Camera _Camera;
        private View _View;
    }
}