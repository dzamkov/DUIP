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

        public override Disposable<Control> CreateControl(ControlEnvironment Environment)
        {
            Rectangle sr = this._SizeRange;
            Rectangle csr = Environment.SizeRange;
            Rectangle nsr = new Rectangle(
                Math.Max(csr.Left, Math.Min(csr.Right, sr.Left)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, sr.Top)),
                Math.Max(csr.Left, Math.Min(csr.Right, sr.Right)),
                Math.Max(csr.Top, Math.Min(csr.Bottom, sr.Bottom)));
            return this._Inner.CreateControl(new ControlEnvironment(Environment)
            {
                SizeRange = nsr
            });
        }

        private Block _Inner;
        private Rectangle _SizeRange;
    }
}