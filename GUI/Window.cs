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
            this._TestTex = Texture.Create(Program.Icon.ToBitmap());
            this._Background = new OceanBackground(new Random());
            
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
            View v = this._Camera.GetView(this.Width, this.Height);
            v.Setup();

            this._Background.Render(v);
            this._TestTex.Bind();
            Texture.DrawQuad(new Rectangle(-1.0, -1.0, 1.0, 1.0));

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            this._Background.Update(updatetime);

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


            if (this.Keyboard[Key.W]) this._Camera.Center.Y -= updatetime;
            if (this.Keyboard[Key.S]) this._Camera.Center.Y += updatetime;

            if (this.Keyboard[Key.A]) this._Camera.Center.X -= updatetime;
            if (this.Keyboard[Key.D]) this._Camera.Center.X += updatetime;

            if (this.Keyboard[Key.Q]) this._Camera.Size *= Math.Pow(2.0, -updatetime);
            if (this.Keyboard[Key.E]) this._Camera.Size *= Math.Pow(2.0, updatetime);
        }

        private Background _Background;
        private Texture _TestTex;
        private Camera _Camera;
    }
}