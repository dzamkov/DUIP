using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that takes up the least amount of space possible without displaying any contents.
    /// </summary>
    public class SpaceBlock : Block
    {
        private SpaceBlock()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly SpaceBlock Singleton = new SpaceBlock();

        public override Disposable<Control> CreateControl(Rectangle SizeRange)
        {
            return new SpaceControl(SizeRange);
        }
    }

    /// <summary>
    /// A control for a space block.
    /// </summary>
    public class SpaceControl : Control
    {
        public SpaceControl(Rectangle SizeRange)
        {
            this._Size = SizeRange.TopLeft;
        }

        public override Point Size
        {
            get
            {
                return this._Size;
            }
        }

        public override Rectangle SizeRange
        {
            set
            {
                this._Size = value.TopLeft;
            }
        }

        public override void Render(RenderContext Context)
        {
            
        }

        private Point _Size;
    }
}