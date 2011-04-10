using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace DUIP.UI.GDI
{
    /// <summary>
    /// A form that displays a control.
    /// </summary>
    public class ControlHost : Form
    {
        public ControlHost(Control Control)
        {
            this._Control = Control;
            this._UpdateSize();

            this.DoubleBuffered = true;
        }

        protected override void OnResize(EventArgs e)
        {
            this._UpdateSize();
            this.Refresh();
        }

        private void _UpdateSize()
        {
            var size = this.ClientSize;
            this._Control.Size = new Point(size.Width, size.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this._Control.Render(new GDIRenderInterface(e.Graphics));
        }

        /// <summary>
        /// Updates and redraws the hosted control by the given amount of time.
        /// </summary>
        public void Update(double Time)
        {
            this._Control.Update(new _UpdateInterface(Time, this));
        }

        /// <summary>
        /// Update interface for a surface.
        /// </summary>
        private class _UpdateInterface : UpdateInterface
        {
            public _UpdateInterface(double Time, ControlHost Host)
            {
                this._Time = Time;
                this._Host = Host;
            }

            public override double Time
            {
                get
                {
                    return this._Time;
                }
            }

            public override void Invalidate(Rectangle Area)
            {
                Point size = Area.Size.Ceiling;

                this._Host.Invalidate(new System.Drawing.Rectangle(
                    (int)Area.Left, (int)Area.Top, (int)size.X, (int)size.Y));
            }

            private ControlHost _Host;
            private double _Time;
        }

        /// <summary>
        /// Creates and displays a control host for a control.
        /// </summary>
        public static void Run(Control Control)
        {
            ControlHost ch = new ControlHost(Control);
            ch.Show();
            DateTime dt = DateTime.Now;
            while (true)
            {
                DateTime nt = DateTime.Now;
                double updatetime = (nt - dt).TotalSeconds;
                dt = nt;

                ch.Update(updatetime);
                Application.DoEvents();
                Thread.Sleep(1);
            }
        }

        private Control _Control;
    }
}