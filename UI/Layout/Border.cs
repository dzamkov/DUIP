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

        public BorderBlock(Block Inner, Border Border)
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

        public override Control CreateControl(Point Size, ControlEnvironment Environment)
        {
            return new BorderControl(this, Size, Environment);
        }

        private Block _Inner;
        private Border _Border;
    }

    /// <summary>
    /// A control for a border block.
    /// </summary>
    public class BorderControl : Control
    {
        public BorderControl(BorderBlock Block, Point Size, ControlEnvironment Environment)
        {
            this._Size = Size;
            this._Border = Block.Border;
            this._BorderVisible = Environment.Borders.Map(x => x.Style == BorderStyle.None);
            this._Inner = Block.Inner.CreateControl(this._InnerSize, new ControlEnvironment()
            {
                Borders = Environment.Borders.Map(this._BorderVisible, (x, y) => y ? this._Border : x)
            });
        }

        public override Point Size
        {
            get
            {
                return this._Size;
            }
        }

        public override Point PreferedSize
        {
            get
            {
                Compass<double> bords = this._BorderWeights;
                return
                    this._Inner.PreferedSize +
                    new Point(bords.Left, bords.Up) +
                    new Point(bords.Right, bords.Down);
            }
        }

        public override void Finish()
        {
            this._Inner.Finish();
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            this._Inner.Update(this._InnerOffset, Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            // Draw inner
            using (Context.Translate(this._InnerOffset))
            {
                this._Inner.Render(Context);
            }

            // Draw borders
            Point size = this._Size;
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

        public override Control Resize(Point Size)
        {
            this._Size = Size;
            this._Inner = this._Inner.Resize(this._InnerSize);
            return this;
        }

        /// <summary>
        /// Gets the offset of the inner control.
        /// </summary>
        private Point _InnerOffset
        {
            get
            {
                Compass<double> bords = this._BorderWeights;
                return new Point(bords.Left, bords.Up);
            }
        }

        /// <summary>
        /// Gets the size of the inner control.
        /// </summary>
        private Point _InnerSize
        {
            get
            {
                Compass<double> bords = this._BorderWeights;
                return this._Size - new Point(bords.Left, bords.Up) - new Point(bords.Right, bords.Down);
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

        private Point _Size;
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
            Style = BorderStyle.None,
            Weight = 0.0
        };

        /// <summary>
        /// The color of the border.
        /// </summary>
        public Color Color;

        /// <summary>
        /// The style of this border.
        /// </summary>
        public BorderStyle Style;

        /// <summary>
        /// The thickness of the border.
        /// </summary>
        public double Weight;
    }

    /// <summary>
    /// A type of border.
    /// </summary>
    public enum BorderStyle
    {
        None,
        Solid,
        Dashed,
        Dotted
    }
}