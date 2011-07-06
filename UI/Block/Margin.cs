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

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            Compass<double> margin = this._Margin;
            Point sizepadding = new Point(margin.Left + margin.Right, margin.Up + margin.Down);
            Layout inner = this.Inner.CreateLayout(SizeRange.Translate(-sizepadding), out Size);
            Size += sizepadding;
            return new _Layout
            {
                Offset = new Point(margin.Left, margin.Up),
                Inner = inner
            };
        }

        public override event Action<Block> LayoutUpdate
        {
            add
            {
                this.Inner.LayoutUpdate += value;
            }
            remove
            {
                this.Inner.LayoutUpdate -= value;
            }
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                this.Inner.Update(Offset + this.Offset, Probes, Time);
            }

            public override void Render(RenderContext Context)
            {
                using (Context.Translate(this.Offset))
                {
                    this.Inner.Render(Context);
                }
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