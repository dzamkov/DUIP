using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that takes up a certain amount of space and does without displaying any contents.
    /// </summary>
    public class SpaceBlock : Block
    {
        public SpaceBlock()
        {

        }

        public SpaceBlock(Point Size)
        {
            this._Size = Size;
        }

        /// <summary>
        /// Gets or sets the size of the block.
        /// </summary>
        public Point Size
        {
            get
            {
                return this._Size;
            }
            set
            {
                this._Size = value;
            }
        }

        public override Control CreateControl(Point Size, ControlEnvironment Environment)
        {
            return new SpaceControl(Size, this._Size);
        }

        private Point _Size;
    }

    /// <summary>
    /// A control for a space block.
    /// </summary>
    public class SpaceControl : Control
    {
        public SpaceControl(Point Size, Point TargetSize)
        {
            this._Size = Size;
            this._TargetSize = TargetSize;
        }

        public override Rectangle Bounds
        {
            get
            {
                return Rectangle.Null;
            }
        }

        public override Point Size
        {
            get
            {
                return this._Size;
            }
        }

        public override Point PreferedSize
        {
            get
            {
                return this._TargetSize;
            }
        }

        public override Control Resize(Point Size)
        {
            this._Size = Size;
            return this;
        }

        public override void Render(View View)
        {
            
        }

        private Point _Size;
        private Point _TargetSize;
    }
}