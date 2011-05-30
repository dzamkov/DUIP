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

        public override Disposable<Control> CreateControl(Rectangle SizeRange)
        {
            BackgroundBlock bb = this._Inner as BackgroundBlock;
            if (bb != null)
            {
                return CreateBorderBackgroundControl(this._Border, bb.Color, SizeRange, bb.Inner);
            }

            return new BorderControl(this._Border, this._Inner.CreateControl(GetInnerSizeRange(SizeRange, this._Border)));
        }

        /// <summary>
        /// Creates a control that combines a border and a background.
        /// </summary>
        public static Disposable<Control> CreateBorderBackgroundControl(Border Border, Color Background, Rectangle SizeRange, Block Inner)
        {
            return new BorderBackgroundControl(Border, Background, Inner.CreateControl(GetInnerSizeRange(SizeRange, Border)));
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

        /// <summary>
        /// Gets the border this control applies.
        /// </summary>
        public Border Border
        {
            get
            {
                return this._Border;
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
            RenderBorders(Context, this._Border.Color, size, w);
        }

        /// <summary>
        /// Draws borders with the given parameters.
        /// </summary>
        public static void RenderBorders(RenderContext Context, Color Color, Point Size, double Weight)
        {
            double hw = Weight * 0.5;
            Context.ClearTexture();
            Context.SetColor(Color);
            using (Context.DrawLines(Weight))
            {
                Context.OutputLine(new Point(hw, 0.0), new Point(hw, Size.Y));
                Context.OutputLine(new Point(0.0, hw), new Point(Size.X, hw));
                Context.OutputLine(new Point(Size.X - hw, 0.0), new Point(Size.X - hw, Size.Y));
                Context.OutputLine(new Point(0.0, Size.Y - hw), new Point(Size.X, Size.Y - hw));
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
    /// A control that includes both a border and a background combined for improved efficency and fewer visual artifacts.
    /// </summary>
    public class BorderBackgroundControl : BorderControl
    {
        public BorderBackgroundControl(Border Border, Color Background, Disposable<Control> Inner)
            : base(Border, Inner)
        {
            this._Background = Background;
        }

        /// <summary>
        /// Gets the background this control applies.
        /// </summary>
        public Color Background
        {
            get
            {
                return this._Background;
            }
        }

        public override void Render(RenderContext Context)
        {
            double w = this.Border.Weight;
            double dw = w * 2.0;
            double hw = w * 0.5;
            Point size = this.Inner.Size + new Point(dw, dw);

            Context.ClearTexture();
            Context.SetColor(this._Background);
            Context.DrawQuad(new Rectangle(hw, hw, size.X - hw, size.Y - hw));
            using (Context.Translate(new Point(w, w)))
            {
                this.Inner.Render(Context);
            }
            RenderBorders(Context, this.Border.Color, size, w);
        }

        private Color _Background;
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