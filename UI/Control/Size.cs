using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that limits the range of prefered sizes an inner control may take.
    /// </summary>
    public class SizeControl : Control, IDisposable
    {
        public SizeControl(Rectangle LimitSizeRange, Disposable<Control> Inner)
        {
            this._LimitSizeRange = LimitSizeRange;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner control for this size control.
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

        private Disposable<Control> _Inner;
        private Rectangle _LimitSizeRange;
    }
}