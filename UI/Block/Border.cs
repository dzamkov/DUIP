using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that applies a border to an inner block.
    /// </summary>
    public class BorderBlock : Block
    {
        public BorderBlock(Border Border, Block Inner)
        {
            this.Border = Border;
            this.Inner = Inner;
        }

        /// <summary>
        /// The border this block applies.
        /// </summary>
        public readonly Border Border;

        /// <summary>
        /// The inner block for this border block.
        /// </summary>
        public readonly Block Inner;

        /// <summary>
        /// Gets the size that is added to the inner block when the border is applied.
        /// </summary>
        public Point SizePadding
        {
            get
            {
                double w = this.Border.Weight * 2.0;
                return new Point(w, w);
            }
        }

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
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
            public override RemoveHandler Link(Context Context)
            {
                double iw = -this.Block.Border.Weight;
                return this.Inner.Link(Context.Translate(new Point(iw, iw)));
            }

            public override Figure Figure
            {
                get
                {
                    Border border = this.Block.Border;
                    double hw = border.Weight * 0.5;
                    return
                        new ShapeFigure(
                            new PathShape(
                                border.Weight,
                                new RectanglePath(new Rectangle(hw, hw, this.Size.X - hw, this.Size.Y - hw))),
                            new SolidFigure(border.Color))
                        ^ Figure.Translate(this.Inner.Figure, new Point(border.Weight, border.Weight));
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