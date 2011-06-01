using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that applies a border to an inner control.
    /// </summary>
    public class BorderControl : Control, IDisposable
    {
        public BorderControl()
        {

        }

        public BorderControl(Border Border, Disposable<Control> Inner)
        {
            this._Border = Border;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the border this control applies.
        /// </summary>
        public Border Border
        {
            get
            {
                return this._Border;
            }
            set
            {
                this._Border = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner control for this border control.
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

        /// <summary>
        /// Gets the size that is added to the inner control when the border is applied.
        /// </summary>
        public Point SizePadding
        {
            get
            {
                double w = this._Border.Weight * 2.0;
                return new Point(w, w);
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            Point sizepadding = this.SizePadding;
            Layout inner = this._Inner.Object.CreateLayout(SizeRange.Translate(-this.SizePadding), out Size);
            Size += sizepadding;
            return new _Layout
            {
                Control = this,
                Inner = inner,
                Size = Size
            };
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                double w = this.Control.Border.Weight;
                this.Inner.Update(new Point(w, w), Probes, Time);
            }

            public override void Render(RenderContext Context)
            {
                Border bord = this.Control.Border;
                using (Context.Translate(new Point(bord.Weight, bord.Weight)))
                {
                    this.Inner.Render(Context);
                }
                bord.Render(Context, this.Size);
            }

            public BorderControl Control;
            public Layout Inner;
            public Point Size;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Disposable<Control> _Inner;
        private Border _Border;
    }

    /// <summary>
    /// Information about a boundary between two areas.
    /// </summary>
    public struct Border
    {
        /// <summary>
        /// An invisible border.
        /// </summary>
        public static readonly Border None = new Border()
        {
            Color = Color.Transparent,
            Weight = 0.0
        };

        /// <summary>
        /// Renders a perimeter to the given render context using this border.
        /// </summary>
        /// <param name="Size">The size of the outer boundary of the perimeter.</param>
        public void Render(RenderContext Context, Point Size)
        {
            Context.ClearTexture();
            Context.SetColor(this.Color);
            double hw = this.Weight * 0.5;
            using (Context.DrawLines(this.Weight))
            {
                Context.OutputLine(new Point(hw, 0.0), new Point(hw, Size.Y));
                Context.OutputLine(new Point(0.0, hw), new Point(Size.X, hw));
                Context.OutputLine(new Point(Size.X - hw, 0.0), new Point(Size.X - hw, Size.Y));
                Context.OutputLine(new Point(0.0, Size.Y - hw), new Point(Size.X, Size.Y - hw));
            }
        }

        /// <summary>
        /// The color of the border.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The thickness of the border.
        /// </summary>
        public double Weight;
    }
}