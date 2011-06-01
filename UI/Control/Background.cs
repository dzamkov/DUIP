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
        public BackgroundControl()
        {

        }

        public BackgroundControl(Color Color, Disposable<Control> Inner)
        {
            this._Color = Color;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the color of the background applied by this control.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                this._Color = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner control for this background control. This control is displayed above the background.
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
            return new _Layout
            {
                Control = this,
                Inner = this._Inner.Object.CreateLayout(SizeRange, out Size),
                Size = Size
            };
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                this.Inner.Update(Offset, Probes, Time);
            }

            public override void Render(RenderContext Context)
            {
                Context.ClearTexture();
                Context.SetColor(this.Control.Color);
                Context.DrawQuad(new Rectangle(Point.Origin, this.Size));
                this.Inner.Render(Context);
            }

            public BackgroundControl Control;
            public Layout Inner;
            public Point Size;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Color _Color;
        private Disposable<Control> _Inner;
    }
}