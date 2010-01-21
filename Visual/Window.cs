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
            this.WindowBorder = WindowBorder.Fixed;

            World w = new World();
            View v = new View();
            v.Location = Sector.Create(w).Center;
            v.ZoomLevel = 0.7;
            TestSection.CreateIntrestingEnvironment(v.Location.Sector, 0);

            this._Drawer = new Drawer();
            this._Drawer.View = v;
            this._Drawer.UpdateView();

            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            this._Drawer.Draw();
            this.SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            this._Drawer.Width = (double)this.Width;
            this._Drawer.Height = (double)this.Height;
            this._Drawer.UpdateView();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //Input
            if (this.Keyboard[Key.Escape])
            {
                this.Close();
            }
            View v = this._Drawer.View;
            if (this.Keyboard[Key.Q]) v.Zoom(0.99);
            if (this.Keyboard[Key.E]) v.Zoom(1.01);
            if (this.Keyboard[Key.A]) v.Pan(-0.01, 0.0);
            if (this.Keyboard[Key.D]) v.Pan(0.01, 0.0);
            if (this.Keyboard[Key.W]) v.Pan(0.0, -0.01);
            if (this.Keyboard[Key.S]) v.Pan(0.0, 0.01);
            v.Normalize();
            this._Drawer.View = v;
            this._Drawer.UpdateView();
        }

        private Drawer _Drawer;
    }
}
