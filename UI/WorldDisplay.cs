using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
            this._World = new World();
            this._Probe = new Probe();
            this._MakeView();

            StaticContent<Data> testcontent = new StaticContent<Data>(null, new DataType());
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
            this._Probe.UpdatePosition(this._GetMousePosition(e.X, e.Y));
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

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this._WheelDelta += e.Delta;
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            this._HasProbe = true;
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

            public override void Lock()
            {
                this._Lock = this._User;
            }

            public override void Release()
            {
                this._Lock = null;
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
        /// Gets the position of the mouse in world coordinates by the given offset in the client area of the view.
        /// </summary>
        private Point _GetMousePosition(int X, int Y)
        {
            return this._View.Project(new Point(X / (double)this.Width, Y / (double)this.Height));
        }

        private int _WheelDelta;

        private Probe _Probe;
        private bool _HasProbe;

        private Ambience _Background;
        private World _World;
        private Camera _Camera;
        private View _View;
    }
}