using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace DUIP.GUI
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

            this._Camera = new Camera(new Point(0.0, 0.0), 1.0);
            this._Background = new OceanBackground(new Random());
            this._World = new World();
            this._MakeView();
            
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Front);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
            this._View.Setup();
            this._Background.Render(this._World, this._View);
            this._World.Render(this._View);

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            this._World.Update(null, updatetime);
            this._Background.Update(this._World, updatetime);

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
            double mrate = rate * this._Camera.Scale;
            double zrate = rate;
            if (this.Keyboard[Key.W]) this._Camera.Velocity.Y -= mrate * updatetime;
            if (this.Keyboard[Key.S]) this._Camera.Velocity.Y += mrate * updatetime;

            if (this.Keyboard[Key.A]) this._Camera.Velocity.X -= mrate * updatetime;
            if (this.Keyboard[Key.D]) this._Camera.Velocity.X += mrate * updatetime;

            if (this.Keyboard[Key.Q]) this._Camera.ZoomVelocity -= zrate * updatetime;
            if (this.Keyboard[Key.E]) this._Camera.ZoomVelocity += zrate * updatetime;

            this._Camera.Update(updatetime, 0.01);
            this._MakeView();
        }

        /// <summary>
        /// Creates the view for the current camera.
        /// </summary>
        private void _MakeView()
        {
            this._View = this._Camera.GetView(this.Width, this.Height);
        }

        private Background _Background;
        private World _World;
        private Camera _Camera;
        private View _View;
    }
}