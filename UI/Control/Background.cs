using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that displays a background below an inner control.
    /// </summary>
    public class BackgroundControl : Control, IDisposable
    {
        public BackgroundControl(Color Color, Disposable<Control> Inner)
        {
            this._Color = Color;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the color of the background applied by this control.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        /// <summary>
        /// Gets the inner control for this background control. This control is displayed above the background.
        /// </summary>
        public Control Inner
        {
            get
            {
                return this._Inner;
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            throw new NotImplementedException();
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                throw new NotImplementedException();
            }

            public override void Render(RenderContext Context)
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Color _Color;
        private Disposable<Control> _Inner;
    }
}