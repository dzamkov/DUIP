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

        public override Disposable<Control> CreateControl(ControlEnvironment Environment)
        {
            return new BorderControl(this, Environment);
        }

        private Block _Inner;
        private Border _Border;
    }

    /// <summary>
    /// A control for a border block.
    /// </summary>
    public class BorderControl : Control, IDisposable
    {
        public BorderControl(BorderBlock Block, ControlEnvironment Environment)
        {
            this._Border = Block.Border;
            this._BorderVisible = Environment.Borders.Map(x => x.Weight == 0.0 || x.Color.A == 0.0);
            this._Inner = Block.Inner.CreateControl(new ControlEnvironment()
            {
                SizeRange = Environment.SizeRange.Translate(-this.InnerSizePadding),
                Borders = Environment.Borders.Map(this._BorderVisible, (x, y) => y ? this._Border : x)
            });
        }

        public override Point Size
        {
            get
            {
                return this._Inner.Size + this.InnerSizePadding;
            }
        }

        /// <summary>
        /// Gets the size that is added to the inner control due to borders. 
        /// </summary>
        public Point InnerSizePadding
        {
            get
            {
                Compass<double> bords = this._BorderWeights;
                return new Point(bords.Left, bords.Up) + new Point(bords.Right, bords.Down);
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            Compass<double> bords = this._BorderWeights;
            this._Inner.Update(Offset + new Point(bords.Left, bords.Up), Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            // Draw inner
            Compass<double> bords = this._BorderWeights;
            using (Context.Translate(new Point(bords.Left, bords.Up)))
            {
                this._Inner.Render(Context);
            }

            // Draw borders
            Point size = this._Inner.Size + new Point(bords.Left, bords.Up) + new Point(bords.Right, bords.Down);
            Compass<bool> vis = this._BorderVisible;
            double w = this._Border.Weight;
            double hw = w * 0.5;

            Context.ClearTexture();
            Context.SetColor(this._Border.Color);
            using (Context.DrawLines(w))
            {
                if (vis.Left) 
                    Context.OutputLine(new Point(hw, 0.0), new Point(hw, size.Y));
                if (vis.Up) 
                    Context.OutputLine(new Point(0.0, hw), new Point(size.X, hw));
                if (vis.Right)
                    Context.OutputLine(new Point(size.X - hw, 0.0), new Point(size.X - hw, size.Y));
                if (vis.Down)
                    Context.OutputLine(new Point(0.0, size.Y - hw), new Point(size.X, size.Y - hw));
            }
        }

        /// <summary>
        /// Gets the weights of the borders on the sides of this control.
        /// </summary>
        private Compass<double> _BorderWeights
        {
            get
            {
                double w = this._Border.Weight;
                return this._BorderVisible.Map(x => x ? w : 0.0);
            }
        }

        public void Dispose()
        {
            ((Disposable<Control>)this._Inner).Dispose();
        }

        private Control _Inner;
        private Border _Border;
        private Compass<bool> _BorderVisible;
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