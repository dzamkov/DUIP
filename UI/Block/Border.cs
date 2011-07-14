using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that applies a border to an inner block.
    /// </summary>
    public class BorderBlock : Block, IDisposable
    {
        public BorderBlock()
        {

        }

        public BorderBlock(Border Border, Disposable<Block> Inner)
        {
            this._Border = Border;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the border this block applies.
        /// </summary>
        [StaticProperty]
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
        /// Gets or sets the inner block for this border block.
        /// </summary>
        [StaticProperty]
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
        /// Gets the size that is added to the inner block when the border is applied.
        /// </summary>
        [StaticProperty]
        public Point SizePadding
        {
            get
            {
                double w = this._Border.Weight * 2.0;
                return new Point(w, w);
            }
        }

        public override Layout CreateLayout(InputContext Context, Rectangle SizeRange, out Point Size)
        {
            Point sizepadding = this.SizePadding;
            Layout inner = this.Inner.CreateLayout(null, SizeRange.Translate(-sizepadding), out Size);
            Size += sizepadding;
            return new _Layout
            {
                Block = this,
                Inner = inner,
                Size = Size
            };
        }

        private class _Layout : Layout
        {
            public override RemoveHandler Link(InputContext Context)
            {
                double iw = -this.Block._Border.Weight;
                return this.Inner.Link(Context.Translate(new Point(iw, iw)));
            }

            public override Figure Figure
            {
                get
                {
                    Border border = this.Block._Border;
                    double hw = border.Weight * 0.5;
                    return
                        new ShapeFigure(
                            new PathShape(
                                border.Weight,
                                new RectanglePath(new Rectangle(hw, hw, this.Size.X - hw, this.Size.Y - hw))),
                            new SolidFigure(border.Color))
                        ^ this.Inner.Figure.Translate(new Point(border.Weight, border.Weight));
                }
            }

            public override RemoveHandler RegisterFigureChange(Action Callback)
            {
                return this.Inner.RegisterFigureChange(Callback);
            }

            public BorderBlock Block;
            public Layout Inner;
            public Point Size;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Disposable<Block> _Inner;
        private Border _Border;
    }

    /// <summary>
    /// Information about a boundary between two areas.
    /// </summary>
    public struct Border
    {
        public Border(double Weight, Color Color)
        {
            this.Weight = Weight;
            this.Color = Color;
        }

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