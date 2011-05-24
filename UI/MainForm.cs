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
    public class MainForm : Form
    {
        public MainForm()
        {
            this.Controls.Add(this._WorldView = new WorldView());
            this._WorldView.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Gets the control used to interact with the world.
        /// </summary>
        public WorldView WorldView
        {
            get
            {
                return this._WorldView;
            }
        }

        /// <summary>
        /// Performs rendering on the form.
        /// </summary>
        /*public void Render()
        {
            this._GLControl.MakeCurrent();
            RenderContext rc = new RenderContext(this._View, this._GLControl.Width, this._GLControl.Height, true);
            this._Background.Render(this._World, rc);
            this._World.Render(rc);
            this.SwapBuffers();
        }

        /// <summary>
        /// Updates the contents of the form by the given amount of time.
        /// </summary>
        public void Update(double Time)
        {
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
            if (this.Keyboard[Key.W]) this._Camera.Velocity.Y -= mrate * Time;
            if (this.Keyboard[Key.S]) this._Camera.Velocity.Y += mrate * Time;

            if (this.Keyboard[Key.A]) this._Camera.Velocity.X -= mrate * Time;
            if (this.Keyboard[Key.D]) this._Camera.Velocity.X += mrate * Time;

            if (this.Keyboard[Key.Q]) this._Camera.ZoomVelocity -= zrate * Time;
            if (this.Keyboard[Key.E]) this._Camera.ZoomVelocity += zrate * Time;

            Point tar = this._View.Project(this.MousePosition);
            this._Camera.ZoomTo(tar, zrate * 0.2 * (this._LastMouseWheel - (this._LastMouseWheel = Mouse.WheelPrecise)));
            this._Camera.Update(updatetime, 0.01, -2.0, 8.0);

            this._MakeView();

            // Update world state
            this._Probe.Update(this.Mouse, tar);
            this._World.Update(new Probe[] { this._Probe }, updatetime);
            this._Background.Update(this._World, updatetime);  
        }*/

        private WorldView _WorldView;
    }
}