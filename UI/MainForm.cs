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
            this.Size = new Size(640, 680);
            this.Controls.Add(this._WorldDisplay = new WorldDisplay());
            this._WorldDisplay.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Gets the control used to interact with the world.
        /// </summary>
        public WorldDisplay WorldDisplay
        {
            get
            {
                return this._WorldDisplay;
            }
        }

        private WorldDisplay _WorldDisplay;
    }
}