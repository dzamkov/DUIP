using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace DUIP.UI
{
    /// <summary>
    /// The main window for the program.
    /// </summary>
    public class Window : GameWindow
    {
        public Window()
            : base(640, 680, GraphicsMode.Default, "DUIP", GameWindowFlags.Default)
        {
            this.Icon = Program.Icon;
            RenderContext.Initialize();

            this._Camera = new Camera(new Point(0.0, 0.0), 1.0);
            this._Background = new OceanAmbience(new Random());
            this._World = new World();
            this._Probe = new Probe();
            this._MakeView();

            Block testblock = new BorderBlock
            {
                Border = new Border
                {
                    Color = Color.RGB(1.0, 0.2, 0.2),
                    Weight = 0.05,
                },
                Inner = new BackgroundBlock
                {
                    Color = Color.RGB(0.95, 0.7, 0.7),
                    Inner = new SpaceBlock
                    {
                        Size = new Point(1.0, 1.0)
                    }
                }
            };
            Control testcontrol = testblock.CreateControl(new Point(1.0, 1.0), new ControlEnvironment());
            testcontrol = testcontrol.Resize(testcontrol.PreferedSize);
            this._TestFigure = testcontrol;
        }

        /// <summary>
        /// Gets the aspect ratio of the window.
        /// </summary>
        public double AspectRatio
        {
            get
            {
                return (double)this.Width / this.Height;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            RenderContext rc = new RenderContext(this._View);
            this._Background.Render(this._World, rc);
            this._World.Render(rc);
            this._TestFigure.Render(rc);

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;

            // Handle input
            if (this.Keyboard[Key.Escape])
            {
                this.Close();
            }

            if (this.Keyboard[Key.F])
            {
                if (this.WindowState == WindowState.Fullscreen)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Fullscreen;
                }
            }

            double rate = 10.0;
            double mrate = rate;
            double zrate = rate;
            if (this.Keyboard[Key.W]) this._Camera.Velocity.Y -= mrate * updatetime;
            if (this.Keyboard[Key.S]) this._Camera.Velocity.Y += mrate * updatetime;

            if (this.Keyboard[Key.A]) this._Camera.Velocity.X -= mrate * updatetime;
            if (this.Keyboard[Key.D]) this._Camera.Velocity.X += mrate * updatetime;

            if (this.Keyboard[Key.Q]) this._Camera.ZoomVelocity -= zrate * updatetime;
            if (this.Keyboard[Key.E]) this._Camera.ZoomVelocity += zrate * updatetime;

            Point tar = this._View.Project(this.MousePosition);
            this._Camera.ZoomTo(tar, zrate * 0.2 * (this._LastMouseWheel - (this._LastMouseWheel = this.Mouse.WheelPrecise)));
            this._Camera.Update(updatetime, 0.01, -2.0, 8.0);

            this._MakeView();

            // Update world state
            this._Probe.Update(this.Mouse, tar);
            this._World.Update(new Probe[] { this._Probe }, updatetime);
            this._Background.Update(this._World, updatetime);  
        }

        /// <summary>
        /// The probe used for a window.
        /// </summary>
        public class Probe : DUIP.UI.Probe
        {
            /// <summary>
            /// Updates the state of the probe based on information from the given window.
            /// </summary>
            public void Update(Window Window)
            {
                this.Update(Window.Mouse, Window._View.Project(Window.MousePosition));
            }

            /// <summary>
            /// Updates the state of the probe based on the given information.
            /// </summary>
            public void Update(MouseDevice Mouse, Point Position)
            {
                this._Pressed = Mouse[MouseButton.Left];
                this._Position = Position;
            }

            public override bool Pressed
            {
                get
                {
                    return this._Pressed;
                }
            }

            public override object Owner
            {
                get
                {
                    return this._Owner;
                }
            }

            public override void Lock(object Owner)
            {
                this._Owner = Owner;
            }

            public override void Release(object Owner)
            {
                this._Owner = null;
            }

            public override Point Position
            {
                get
                {
                    return this._Position;
                }
            }

            private Point _Position;
            private bool _Pressed;
            private object _Owner;
        }

        /// <summary>
        /// Gets the relative position of the mouse on the window.
        /// </summary>
        public Point MousePosition
        {
            get
            {
                MouseDevice md = this.Mouse;
                return new Point(
                    (double)md.X / this.Width,
                    (double)md.Y / this.Height);
            }
        }

        /// <summary>
        /// Creates the view for the current camera.
        /// </summary>
        private void _MakeView()
        {
            this._View = this._Camera.GetView(this.Width, this.Height);
        }

        private Figure _TestFigure;
        private Probe _Probe;
        private Ambience _Background;
        private World _World;
        private Camera _Camera;
        private View _View;
        private float _LastMouseWheel;
    }
}