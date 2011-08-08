using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that applies a margin to an inner block.
    /// </summary>
    public class MarginBlock : Block
    {
        public MarginBlock(Compass<double> Margin, Block Inner)
        {
            this.Margin = Margin;
            this.Inner = Inner;
        }

        /// <summary>
        /// The size of the margin applied by this block.
        /// </summary>
        public readonly Compass<double> Margin;

        /// <summary>
        /// The inner block for this margin block.
        /// </summary>
        public readonly Block Inner;

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            Compass<double> margin = this.Margin;
            Point sizepadding = new Point(margin.Left + margin.Right, margin.Up + margin.Down);
            Layout inner = this.Inner.CreateLayout(null, SizeRange.Translate(-sizepadding), out Size);
            Size += sizepadding;
            return new _Layout
            {
                Offset = new Point(margin.Left, margin.Up),
                Inner = inner
            };
        }

        private class _Layout : Layout
        {
            public override RemoveHandler Link(Context Context)
            {
                return this.Inner.Link(Context.Translate(-this.Offset));
            }

            public override Figure Figure
            {
                get
                {
                    return Figure.Translate(this.Inner.Figure, this.Offset);
                }
            }

            public override RemoveHandler RegisterFigureChange(Action Callback)
            {
                return this.Inner.RegisterFigureChange(Callback);
            }

            public Point Offset;
            public Layout Inner;
        }
    }
}