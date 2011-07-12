using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that displays a background below an inner block.
    /// </summary>
    public class BackgroundBlock : Block, IDisposable
    {
        public BackgroundBlock()
        {

        }

        public BackgroundBlock(Color Color, Disposable<Block> Inner)
        {
            this._Color = Color;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the color of the background applied by this block.
        /// </summary>
        [DynamicProperty]
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
        /// Gets or sets the inner block for this background block. This block is displayed above the background.
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
            BorderBlock bc = this.Inner as BorderBlock;
            if (bc != null)
            {
                return BorderBlock.CreateBorderBackgroundLayout(Context, SizeRange, bc, this, bc.Inner, out Size);
            }
            else
            {
                return new _Layout
                {
                    Block = this,
                    Inner = this.Inner.CreateLayout(null, SizeRange, out Size),
                    Size = Size
                };
            }
        }


        private class _Layout : Layout
        {
            public override void Render(RenderContext Context)
            {
                Context.ClearTexture();
                Context.SetColor(this.Block.Color);
                Context.DrawQuad(new Rectangle(Point.Origin, this.Size));
                this.Inner.Render(Context);
            }

            public BackgroundBlock Block;
            public Layout Inner;
            public Point Size;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Color _Color;
        private Disposable<Block> _Inner;
    }
}