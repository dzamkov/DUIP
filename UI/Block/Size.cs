using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block which limits the available sizes for an inner control.
    /// </summary>
    public class SizeBlock : Block
    {
        public SizeBlock()
        {

        }

        public SizeBlock(Rectangle SizeRange, Block Inner)
        {
            this._SizeRange = SizeRange;
            this._Inner = Inner;
        }

        public SizeBlock(Point Size, Block Inner)
        {
            this._SizeRange = new Rectangle(Size, Size);
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the inner block.
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
        /// Gets or sets the size range to use for this block.
        /// </summary>
        public Rectangle SizeRange
        {
            get
            {
                return this._SizeRange;
            }
            set
            {
                this._SizeRange = value;
            }
        }

        public override Disposable<Control> CreateControl(Rectangle SizeRange, Theme Theme)
        {
            return new SizeControl(this._SizeRange, SizeRange, this._Inner.CreateControl(GetInnerSizeRange(this._SizeRange, SizeRange), Theme));
        }

        /// <summary>
        /// Gets the size range of an inner control given the size range the block restricts to.
        /// </summary>
        public static Rectangle GetInnerSizeRange(Rectangle LimitSizeRange, Rectangle SizeRange)
        {
            Rectangle lsr = LimitSizeRange;
            Rectangle csr = SizeRange;
            return new Rectangle(
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Left)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Top)),
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Right)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Bottom)));
        }

        private Block _Inner;
        private Rectangle _SizeRange;
    }

    /// <summary>
    /// A control for a size block.
    /// </summary>
    public class SizeControl : Control, IDisposable
    {
        public SizeControl(Rectangle LimitSizeRange, Rectangle SizeRange, Disposable<Control> Inner)
        {
            this._LimitSizeRange = LimitSizeRange;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner control for this size control.
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
                return this.Inner.Size;
            }
        }

        public override Rectangle SizeRange
        {
            set
            {
                this.Inner.SizeRange = SizeBlock.GetInnerSizeRange(this._LimitSizeRange, value);    
            }
        }

        public override Theme Theme
        {
            set
            {
                this.Inner.Theme = value;
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            this.Inner.Update(Offset, Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            this.Inner.Render(Context);
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Disposable<Control> _Inner;
        private Rectangle _LimitSizeRange;
    }
}