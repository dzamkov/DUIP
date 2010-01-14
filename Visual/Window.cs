using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using GraphicsMode = OpenTK.Graphics.GraphicsMode;

namespace DUIP.Visual
{
    /// <summary>
    /// Main window used to show the world.
    /// </summary>
    public class Window : GameWindow
    {
        public Window(DisplayDevice Device)
            : base(1440, 900, GraphicsMode.Default, "DUIP",
                GameWindowFlags.Fullscreen, Device)
        {
            this._Drawer = new Drawer();

            this._View = new View();
            this._View.Location = new Core.GeneralSector(new Core.LVector { Down = 2, Right = 2 }).Center;
            this._View.ZoomLevel = 0.7;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // Exit on q
            if (e.KeyChar == 'q')
            {
                this.Close();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            this._Drawer.Render(this._View, (double)this.Width / (double)this.Height);
            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this._View.Zoom(0.99);
        }

        private View _View;
        private Drawer _Drawer;
    }
}
