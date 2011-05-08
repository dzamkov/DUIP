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

        public BorderBlock(Block Inner, Compass<Border> Borders)
        {
            this._Inner = Inner;
            this._Borders = Borders;
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
        /// Gets or sets the borders this block applies.
        /// </summary>
        public Compass<Border> Borders
        {
            get
            {
                return this._Borders;
            }
            set
            {
                this._Borders = value;
            }
        }

        public override Control CreateControl(Point Size, ControlEnvironment Environment)
        {
            return new BorderControl(this, Size, Environment);
        }

        private Block _Inner;
        private Compass<Border> _Borders;
    }

    /// <summary>
    /// A control for a border block.
    /// </summary>
    public class BorderControl : Control
    {
        public BorderControl(BorderBlock Block, Point Size, ControlEnvironment Environment)
        {
            this._Size = Size;
            this._VisibleBorders = Block.Borders.Map(Environment.Borders, (x, y) => y.Style == BorderStyle.None ? x : Border.None);
            Point innersize, inneroffset; _CalculateInnerLayout(this._Size, this._VisibleBorders, out innersize, out inneroffset);
            this._Inner = Block.Inner.CreateControl(innersize, new ControlEnvironment()
            {
                Borders = this._VisibleBorders.Map(Environment.Borders, (x, y) => x.Style == BorderStyle.None ? y : x)
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
                Compass<Border> bords = this._VisibleBorders;
                return
                    this._Inner.PreferedSize +
                    new Point(bords.Left.Weight, bords.Up.Weight) +
                    new Point(bords.Right.Weight, bords.Down.Weight);
            }
        }

        public override void Finish()
        {
            this._Inner.Finish();
        }

        public override void Render(View View)
        {
            throw new NotImplementedException();
        }

        public override Control Resize(Point Size)
        {
            Point innersize, inneroffset;
            this._Size = Size;
            _CalculateInnerLayout(this._Size, this._VisibleBorders, out innersize, out inneroffset);
            this._Inner = this._Inner.Resize(innersize);
            return this;
        }

        /// <summary>
        /// Calculates the layout for the inner control.
        /// </summary>
        internal static void _CalculateInnerLayout(Point Size, Compass<Border> Borders, out Point InnerSize, out Point InnerOffset)
        {
            InnerOffset = new Point(Borders.Left.Weight, Borders.Up.Weight);
            InnerSize = Size - new Point(Borders.Right.Weight, Borders.Down.Weight) - InnerOffset;
        }

        private Point _Size;
        private Control _Inner;
        private Compass<Border> _VisibleBorders;
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