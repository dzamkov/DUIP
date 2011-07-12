using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that applies a margin to an inner block.
    /// </summary>
    public class MarginBlock : Block, IDisposable
    {
        public MarginBlock()
        {

        }

        public MarginBlock(Compass<double> Margin, Disposable<Block> Inner)
        {
            this._Margin = Margin;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the size of the margin applied by this block.
        /// </summary>
        [StaticProperty]
        public Compass<double> Margin
        {
            get
            {
                return this._Margin;
            }
            set
            {
                this._Margin = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner block for this margin block.
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

        public override Layout CreateLayout(InputContext Context, Rectangle SizeRange, out Point Size)
        {
            Compass<double> margin = this._Margin;
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
            public override void Render(RenderContext Context)
            {
                using (Context.Translate(this.Offset))
                {
                    this.Inner.Render(Context);
                }
            }

            public override RemoveHandler RegisterInvalidate(Action Callback)
            {
                return this.Inner.RegisterInvalidate(Callback);
            }

            public Point Offset;
            public Layout Inner;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Compass<double> _Margin;
        private Disposable<Block> _Inner;
    }
}