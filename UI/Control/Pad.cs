using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that applies padding to an inner control.
    /// </summary>
    public class PadControl : Control, IDisposable
    {
        public PadControl(Compass<double> Padding, Disposable<Control> Inner)
        {
            this._Padding = Padding;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner control for this pad control.
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

        private Compass<double> _Padding;
        private Disposable<Control> _Inner;
    }
}