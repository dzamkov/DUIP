using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that insets an inner block with a padding area.
    /// </summary>
    public class PadBlock : Block
    {
        public PadBlock()
        {

        }

        public PadBlock(Compass<double> Padding, Block Inner)
        {
            this._Padding = Padding;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the amount of padding on each edge of the inner block.
        /// </summary>
        public Compass<double> Padding
        {
            get
            {
                return this._Padding;
            }
            set
            {
                this._Padding = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner block to which the padding is applied to.
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

        public override Disposable<Control> CreateControl(Rectangle SizeRange)
        {
            Compass<double> pad = this._Padding;
            return new PadControl(pad, this._Inner.CreateControl(GetInnerSizeRange(SizeRange, pad)));
        }

        /// <summary>
        /// Gets the size range to use for an inner control, given the amount of padding.
        /// </summary>
        public static Rectangle GetInnerSizeRange(Rectangle SizeRange, Compass<double> Padding)
        {
            return SizeRange.Translate(-new Point(Padding.Left + Padding.Right, Padding.Up + Padding.Down));
        }

        private Compass<double> _Padding;
        private Block _Inner;
    }

    /// <summary>
    /// A control for a pad block.
    /// </summary>
    public class PadControl : Control, IDisposable
    {
        public PadControl(Compass<double> Padding, Disposable<Control> Inner)
        {
            this._Padding = Padding;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner control for this pad control.
        /// </summary>
        public Control Inner
        {
            get
            {
                return this._Inner;
            }
        }

        public override Point Size
        {
            get
            {
                Compass<double> pad = this._Padding;
                return this.Inner.Size +
                    new Point(pad.Left, pad.Up) +
                    new Point(pad.Right, pad.Down);
            }
        }

        public override Rectangle SizeRange
        {
            set
            {
                this.Inner.SizeRange = PadBlock.GetInnerSizeRange(value, this._Padding);
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            Compass<double> pad = this._Padding;
            this.Inner.Update(Offset + new Point(pad.Left, pad.Up), Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            Compass<double> pad = this._Padding;
            using (Context.Translate(new Point(pad.Left, pad.Up)))
            {
                this.Inner.Render(Context);
            }
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Compass<double> _Padding;
        private Disposable<Control> _Inner;
    }
}