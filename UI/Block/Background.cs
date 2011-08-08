using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that displays a background below an inner block.
    /// </summary>
    public class BackgroundBlock : Block
    {
        public BackgroundBlock(Color Color, Block Inner)
        {
            this._Color = Color;
            this.Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the color of the background applied by this block.
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
        /// The inner block for this background block. This block is displayed above the background.
        /// </summary>
        public readonly Block Inner;

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            return new _Layout
            {
                Block = this,
                Inner = this.Inner.CreateLayout(null, SizeRange, out Size),
                Size = Size
            };
        }

        private class _Layout : Layout
        {
            public override RemoveHandler Link(Context Context)
            {
                return this.Inner.Link(Context);
            }

            public override Figure Figure
            {
                get
                {
                    return
                        new ShapeFigure(
                            new RectangleShape(new Rectangle(Point.Origin, this.Size)),
                            new SolidFigure(this.Block._Color))
                        + this.Inner.Figure;
                }
            }

            public override RemoveHandler RegisterFigureChange(Action Callback)
            {
                return this.Inner.RegisterFigureChange(Callback);
            }

            public BackgroundBlock Block;
            public Layout Inner;
            public Point Size;
        }

        private Color _Color;
    }
}