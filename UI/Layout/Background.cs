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

        public override Control CreateControl(Point Size, ControlEnvironment Environment)
        {
            return new BackgroundControl(this._Color, this._Inner.CreateControl(Size, Environment));
        }

        private Color _Color;
        private Block _Inner;
    }

    /// <summary>
    /// A control for a background block.
    /// </summary>
    public class BackgroundControl : Control
    {
        public BackgroundControl(Color Color, Control Inner)
        {
            this._Color = Color;
            this._Inner = Inner;
        }

        public override Point Size
        {
            get
            {
                return this._Inner.Size;
            }
        }

        public override Point PreferedSize
        {
            get
            {
                return this._Inner.PreferedSize;
            }
        }

        public override Control Resize(Point Size)
        {
            this._Inner = this._Inner.Resize(Size);
            return this;
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            this._Inner.Update(Offset, Probes, Time);
        }

        public override void Render(RenderContext Context)
        {
            Context.ClearTexture();
            Context.SetColor(this._Color);
            Context.DrawQuad(new Rectangle(Point.Origin, this._Inner.Size));
            this._Inner.Render(Context);
        }

        private Color _Color;
        private Control _Inner;
    }
}