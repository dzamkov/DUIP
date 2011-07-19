using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using WPoint = System.Drawing.Point;

using DUIP.UI.Graphics;
using DUIP.UI.Render;

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

            this._Renderer = new Renderer();
            this._Renderer.Initialize();
            SystemTypeface.Create = this._Renderer.CreateSystemTypeface;

            this._Probe = new Probe();
            this._InputContext = new InputContext(this._Probe);

            this._Camera = new Camera(new Point(0.0, 0.0), 1.0);
            this._Background = new OceanAmbience();
            this._World = new World(this._InputContext, new Theme());
            this._MakeView();

            Content testcontent = new EditorContent();
            this._World.Spawn(testcontent, Point.Origin);
        }

        /// <summary>
        /// Performs rendering on the view.
        /// </summary>
        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            this.MakeCurrent();
            this._Renderer.Render(this._View, this.Width, this.Height, true, this._World.Figure);
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

            this._InputContext.Update(Time);
            this._Background.Update(this._World, Time);  
        }

        protected override void OnResize(EventArgs e)
        {
            this._MakeView();
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
                this._Probe.UpdateSignal(ProbeSignal.Primary, true);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this._Probe.UpdateSignal(ProbeSignal.Primary, false);
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
        public class Probe : UI.Probe
        {
            /// <summary>
            /// Updates the position of the probe.
            /// </summary>
            public void UpdatePosition(Point Position)
            {
                this._Position = Position;
            }

            /// <summary>
            /// Updates the value of a signal for the probe.
            /// </summary>
            public void UpdateSignal(ProbeSignal Signal, bool Value)
            {
                this._Primary = Value;
                if (this._SignalChange != null)
                {
                    this._SignalChange(Signal, Value);
                }
            }

            /// <summary>
            /// Gets wether the probe is locked.
            /// </summary>
            public bool Locked
            {
                get
                {
                    return this._Locked;
                }
            }

            public override Point Position
            {
                get
                {
                    return this._Position;
                }
            }

            public override bool this[ProbeSignal Signal]
            {
                get
                {
                    return this._Primary;
                }
            }

            public override Action Lock()
            {
                this._Locked = true;
                return delegate { this._Locked = false; };
            }

            public override void Focus(Action Lost)
            {
                if (this._Focus != null)
                {
                    this._Focus();
                }
                this._Focus = Lost;
            }
            
            public override RemoveHandler RegisterSignalChange(Action<ProbeSignal, bool> Callback)
            {
                this._SignalChange += Callback;
                return delegate { this._SignalChange -= Callback; };
            }
            private Action<ProbeSignal, bool> _SignalChange;

            private Point _Position;
            private Action _Focus;
            private bool _Locked;
            private bool _Primary;
        }

        /// <summary>
        /// The input context used for a world view.
        /// </summary>
        public class InputContext : UI.InputContext
        {
            public InputContext(Probe Probe)
            {
                this._Probe = Probe;
            }

            /// <summary>
            /// Performs update events.
            /// </summary>
            /// <param name="Time">The time in seconds since the last update.</param>
            public void Update(double Time)
            {
                if (this._Update != null)
                {
                    this._Update(Time);
                }
            }

            public override RemoveHandler RegisterProbeSignalChange(ProbeSignalChangeHandler Callback)
            {
                this._ProbeSignalChange += Callback;
                if (this._ProbeSignalChange != null && this._RemoveSignalChange == null)
                {
                    Probe probe = this._Probe;
                    this._RemoveSignalChange = probe.RegisterSignalChange(delegate(ProbeSignal Signal, bool Value)
                    {
                        bool handled = false;
                        this._ProbeSignalChange(probe, Signal, Value, ref handled);
                    });
                }
                return delegate
                {
                    this._ProbeSignalChange -= Callback;
                    if (this._ProbeSignalChange == null && this._RemoveSignalChange != null)
                    {
                        this._RemoveSignalChange();
                        this._RemoveSignalChange = null;
                    }
                };
            }
            private ProbeSignalChangeHandler _ProbeSignalChange;
            private RemoveHandler _RemoveSignalChange;

            public override IEnumerable<UI.Probe> Probes
            {
                get
                {
                    if (this._Probe.Locked)
                    {
                        return new UI.Probe[0];
                    }
                    else
                    {
                        return new UI.Probe[] { this._Probe };
                    }
                }
            }

            public override RemoveHandler RegisterUpdate(Action<double> Callback)
            {
                this._Update += Callback;
                return delegate { this._Update -= Callback; };
            }

            private Action<double> _Update;
            private Probe _Probe;
        }

        /// <summary>
        /// Updates the current view for the camera state.
        /// </summary>
        private void _MakeView()
        {
            this._View = this._Camera.GetView(this.AspectRatio);
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
        private InputContext _InputContext;

        private Ambience _Background;
        private World _World;
        private Camera _Camera;
        private View _View;
        private Renderer _Renderer;
    }
}