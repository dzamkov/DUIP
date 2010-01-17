//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using GraphicsMode = OpenTK.Graphics.GraphicsMode;

using DUIP.Core;

namespace DUIP.Visual
{
    /// <summary>
    /// Main window used to show the world.
    /// </summary>
    public class Window : GameWindow
    {
        public Window(DisplayDevice Device)
            : base(640, 480, GraphicsMode.Default, "DUIP",
                GameWindowFlags.Default, Device)
        {
            this._Drawer = new Drawer();
            this.WindowBorder = WindowBorder.Fixed;

            this._View = new View();
            this._View.Location = GeneralSector.Create(new LVector(2, 2)).Center;
            this._View.ZoomLevel = 0.7;

            new TestSection()._Add(this._View.Location.Sector, new Grid(0.0, 0.0, 1.0, 1.0));
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            this._Drawer.Render(this._View, (double)this.Width / (double)this.Height);
            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //Input
            if (this.Keyboard[Key.Escape])
            {
                this.Close();
            }
            if (this.Keyboard[Key.Q]) this._View.Zoom(0.99);
            if (this.Keyboard[Key.E]) this._View.Zoom(1.01);
            if (this.Keyboard[Key.A]) this._View.Pan(-0.01, 0.0);
            if (this.Keyboard[Key.D]) this._View.Pan(0.01, 0.0);
            if (this.Keyboard[Key.W]) this._View.Pan(0.0, -0.01);
            if (this.Keyboard[Key.S]) this._View.Pan(0.0, 0.01);
            this._View.Normalize();
        }

        private View _View;
        private Drawer _Drawer;
    }
}
