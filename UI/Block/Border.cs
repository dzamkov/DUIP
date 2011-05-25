using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that applies a border to an inner block. Note that if there is already a border on a side of the block, no additional
    /// border will be added.
    /// </summary>
    public class BorderBlock : Block
    {
        public BorderBlock()
        {

        }

        public BorderBlock(Border Border, Block Inner)
        {
            this._Inner = Inner;
            this._Border = Border;
        }

        /// <summary>
        /// Gets or sets the inner block in this border block.
        /// </summary>
        public Block Inner
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
        /// Gets or sets the border this block applies.
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

        public override Disposable<Control> CreateControl(Rectangle SizeRange, Theme Theme)
        {
            return new BorderControl(this._Border, this._Inner.CreateControl(GetInnerSizeRange(SizeRange, this._Border), Theme));
        }

        /// <summary>
        /// Gets the size range to use for an inner control, given the border used.
        /// </summary>
        public static Rectangle GetInnerSizeRange(Rectangle SizeRange, Border Border)
        {
            double w = -Border.Weight * 2.0;
            return SizeRange.Translate(new Point(w, w));
        }

        private Block _Inner;
        private Border _Border;
    }

    /// <summary>
    /// A control for a border block.
    /// </summary>
    public class BorderControl : Control, IDisposable
    {
        public BorderControl(Border Border, Disposable<Control> Inner)
        {
            this._Border = Border;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner control for this border control.
        /// </summary>
        public Control Inner
        {
            get
            {
                return this._Inner;
            }
        }

        public override Point Size
        {
            get
            {
                double w = this._Border.Weight * 2.0;
                return this.Inner.Size + new Point(w, w);
            }
        }

        public override Rectangle SizeRange
        {
            set
            {
                this.Inner.SizeRange = value;
            }
        }

        public override Theme Theme
        {
            set
            {
                this.Inner.Theme = value;
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            double w = this._Border.Weight;
            this.Inner.Update(Offset + new Point(w, w), Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            double w = this._Border.Weight;
            double hw = w * 0.5;

            // Draw inner
            using (Context.Translate(new Point(w, w)))
            {
                this.Inner.Render(Context);
            }

            // Draw borders
            Point size = this.Inner.Size + new Point(w * 2.0, w * 2.0);

            Context.ClearTexture();
            Context.SetColor(this._Border.Color);
            using (Context.DrawLines(w))
            {
                Context.OutputLine(new Point(hw, 0.0), new Point(hw, size.Y));
                Context.OutputLine(new Point(0.0, hw), new Point(size.X, hw));
                Context.OutputLine(new Point(size.X - hw, 0.0), new Point(size.X - hw, size.Y));
                Context.OutputLine(new Point(0.0, size.Y - hw), new Point(size.X, size.Y - hw));
            }
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
        /// The color of the border.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The thickness of the border.
        /// </summary>
        public double Weight;
    }
}