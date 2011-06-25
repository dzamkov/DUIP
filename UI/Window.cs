﻿using System;
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

            BitmapTypeface typeface = BitmapTypeface.Create(BitmapTypeface.GetFamily("Arial"), Font.ASCIICharacters, FontStyle.Regular, 3, 60.0f, 512);
            BitmapFont font = typeface.GetFont(0.05, Color.Black);

            FlowBlock testflow = new FlowBlock
            {
                Style = new FlowStyle
                {
                    Direction = FlowDirection.RightDown,
                    Justification = FlowJustification.Justify,
                    LineAlignment = Alignment.Center,
                    LineSpacing = 0.00,
                    LineSize = 0.01
                }
            };
            testflow.AddText(
                "Lorem ipsum dolor sit amet, consec tetur adipis cing elit. Nunc sus cipit phare tra nunc, " + 
                "sit amet fauc ibus risus sceler isque ac. Etiam condi mentum justo quis dolor vehi cula ac volut pat " + 
                "tortor adi piscing. Donec tinci dunt quam quis orci pel lent esque feug iat. Fusce eget nisi ac mi " + 
                "trist ique port titor. Aliq uam et males uada elit. Suspen disse elei fend hend rerit semper. ", font);

            Block testblock = testflow
                .WithPad(0.05)
                .WithSize(1.0, 1.0)
                .WithBorder(new Border
                    {
                        Color = Color.RGB(0.8, 0.2, 0.2),
                        Weight = 0.04,
                    })
                .WithBackground(Color.RGB(0.9, 0.5, 0.5));

            ControlEnvironment ce = new ControlEnvironment()
            {
                SizeRange = new Rectangle(1.0, 1.0, 3.0, 3.0),
                Borders = new Compass<Border>(Border.None)
            };


            List<Node> nodes = new List<Node>();
            for (int t = 0; t < 10; t++)
            {
                Node node = new Node((Control)testblock.CreateControl(ce), new Point(t * 2.0 - 10.0, 0.0), Point.Zero);
                this._World.Spawn(node);
                nodes.Add(node);
            }

            Arc arc = new Arc(
                new Arc.EndPoint(nodes[5], Direction.Right, 0.5),
                new Arc.EndPoint(nodes[6], Direction.Left, 0.5),
                0.03, Color.RGBA(0.5, 0.2, 0.2, 0.6));
            this._World.Spawn(arc);
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
            RenderContext rc = new RenderContext(this._View, this.Width, this.Height, true);
            this._Background.Render(this._World, rc);
            this._World.Render(rc);

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
                this._User = null;
                this._Active = Mouse[MouseButton.Left];
                this._Position = Position;
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

        private Probe _Probe;
        private Ambience _Background;
        private World _World;
        private Camera _Camera;
        private View _View;
        private float _LastMouseWheel;
    }
}