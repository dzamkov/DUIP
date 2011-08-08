using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that limits the range of prefered sizes an inner block may take.
    /// </summary>
    public class SizeBlock : Block
    {
        public SizeBlock(Rectangle LimitSizeRange, Block Inner)
        {
            this.LimitSizeRange = LimitSizeRange;
            this.Inner = Inner;
        }

        /// <summary>
        /// The size range that the inner block is limited to. The size range for the inner
        /// block will be the intersection of this and the size range given to the size block (with some
        /// clamping to ensure that the resulting size range has a positive area).
        /// </summary>
        public readonly Rectangle LimitSizeRange;

        /// <summary>
        /// The inner block for this size block.
        /// </summary>
        public readonly Block Inner;

        /// <summary>
        /// Gets the limited size range created by this size block when given the full available size range.
        /// </summary>
        public Rectangle GetLimitedSizeRange(Rectangle SizeRange)
        {
            Rectangle lsr = this.LimitSizeRange;
            Rectangle csr = SizeRange;
            return new Rectangle(
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Left)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Top)),
                Math.Max(csr.Left, Math.Min(csr.Right, lsr.Right)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, lsr.Bottom)));
        }

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            return this.Inner.CreateLayout(Context, this.GetLimitedSizeRange(SizeRange), out Size);
        }
    }
}