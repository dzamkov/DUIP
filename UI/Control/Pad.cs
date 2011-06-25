﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that applies padding to an inner control.
    /// </summary>
    public class PadControl : Control, IDisposable
    {
        public PadControl()
        {

        }

        public PadControl(Compass<double> Padding, Disposable<Control> Inner)
        {
            this._Padding = Padding;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the amount of padding applied by this control.
        /// </summary>
        public Compass<double> Padding
        {
            get
            {
                return this._Padding;
            }
            set
            {
                this._Padding = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner control for this pad control.
        /// </summary>
        public Control Inner
        {
            get
            {
                return this._Inner;
            }
            set
            {
                this._Inner = value;
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            Compass<double> padding = this._Padding;
            Point sizepadding = new Point(padding.Left + padding.Right, padding.Up + padding.Down);
            Layout inner = this.Inner.CreateLayout(SizeRange.Translate(-sizepadding), out Size);
            Size += sizepadding;
            return new _Layout
            {
                Offset = new Point(padding.Left, padding.Up),
                Inner = inner
            };
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                this.Inner.Update(Offset + this.Offset, Probes, Time);
            }

            public override void Render(RenderContext Context)
            {
                using (Context.Translate(this.Offset))
                {
                    this.Inner.Render(Context);
                }
            }

            public Point Offset;
            public Layout Inner;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Compass<double> _Padding;
        private Disposable<Control> _Inner;
    }
}