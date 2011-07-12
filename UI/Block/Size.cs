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
        [StaticProperty]
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
        /// Gets the limited size range created by this size block when given the full available size range.
        /// </summary>
        public Rectangle GetLimitedSizeRange(Rectangle SizeRange)
        {
            Rectangle lsr = this._LimitSizeRange;
            Rectangle csr = SizeRange;
            return new Rectangle(
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Left)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Top)),
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Right)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Bottom)));
        }

        public override Layout CreateLayout(InputContext Context, Rectangle SizeRange, out Point Size)
        {
            return this.Inner.CreateLayout(Context, this.GetLimitedSizeRange(SizeRange), out Size);
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Disposable<Block> _Inner;
        private Rectangle _LimitSizeRange;
    }
}