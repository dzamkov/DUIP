using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that limits the range of prefered sizes an inner control may take.
    /// </summary>
    public class SizeControl : Control, IDisposable
    {
        public SizeControl()
        {

        }

        public SizeControl(Rectangle LimitSizeRange, Disposable<Control> Inner)
        {
            this._LimitSizeRange = LimitSizeRange;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the size range that the inner control is limited to. The size range for the inner
        /// control will be the intersection of this and the size range given to the size control (with some
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
        /// Gets or sets the inner control for this size control.
        /// </summary>
        public Control Inner
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

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Disposable<Control> _Inner;
        private Rectangle _LimitSizeRange;
    }
}