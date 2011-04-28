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

            this._View = new View(new Point(0.0, 0.0), 1.0);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
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
            GL.Clear(ClearBufferMask.ColorBufferBit);

            this._View.Setup(this.AspectRatio);

            GL.Begin(BeginMode.Quads);
            GL.Color4(Color.RGB(0.0, 0.0, 1.0));
            GL.Vertex2(0.0, 0.0);
            GL.Vertex2(1.0, 0.0);
            GL.Vertex2(1.0, 1.0);
            GL.Vertex2(0.0, 1.0);
            GL.End();

            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;

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


            if (this.Keyboard[Key.W]) this._View.Center.Y -= updatetime;
            if (this.Keyboard[Key.S]) this._View.Center.Y += updatetime;
            if (this.Keyboard[Key.A]) this._View.Center.X -= updatetime;
            if (this.Keyboard[Key.D]) this._View.Center.X += updatetime;
        }

        private View _View;
    }
}