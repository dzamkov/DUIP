using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that sets the background of an inner block.
    /// </summary>
    public class BackgroundBlock : Block
    {
        public BackgroundBlock()
        {

        }

        public BackgroundBlock(Color Color, Block Inner)
        {
            this._Color = Color;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the background color of the block.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
            set
            {
                this._Color = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner block to which the background is applied to.
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
            BorderBlock bb = this._Inner as BorderBlock;
            if (bb != null)
            {
                return BorderBlock.CreateBorderBackgroundControl(bb.Border, this._Color, SizeRange, bb.Inner);
            }

            return new BackgroundControl(this._Color, this._Inner.CreateControl(SizeRange));
        }

        private Color _Color;
        private Block _Inner;
    }

    /// <summary>
    /// A control for a background block.
    /// </summary>
    public class BackgroundControl : Control, IDisposable
    {
        public BackgroundControl(Color Color, Disposable<Control> Inner)
        {
            this._Color = Color;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner control for this background control. This control is displayed above the background.
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
                this.Inner.SizeRange = value;
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            this.Inner.Update(Offset, Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            Context.ClearTexture();
            Context.SetColor(this._Color);
            Context.DrawQuad(new Rectangle(Point.Origin, this.Inner.Size));

            this.Inner.Render(Context);
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Color _Color;
        private Disposable<Control> _Inner;
    }
}