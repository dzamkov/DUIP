using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that limits the range of prefered sizes an inner block may take.
    /// </summary>
    public class SizeBlock : Block, IDisposable
    {
        public SizeBlock()
        {

        }

        public SizeBlock(Rectangle LimitSizeRange, Disposable<Block> Inner)
        {
            this._LimitSizeRange = LimitSizeRange;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the size range that the inner block is limited to. The size range for the inner
        /// block will be the intersection of this and the size range given to the size block (with some
        /// clamping to ensure that the resulting size range has a positive area).
        /// </summary>
        public Rectangle LimitSizeRange
        {
            get
            {
                return this._LimitSizeRange;
            }
            set
            {
                this._LimitSizeRange = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner block for this size block.
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
            Rectangle lsr = LimitSizeRange;
            Rectangle csr = SizeRange;
            Rectangle sr = new Rectangle(
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Left)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Top)),
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Right)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Bottom)));
            return this._Inner.Object.CreateLayout(sr, out Size);
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

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Disposable<Block> _Inner;
        private Rectangle _LimitSizeRange;
    }
}