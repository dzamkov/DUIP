using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A content representation static instance of a certain type, possibly mirrored on a network.
    /// </summary>
    public class StaticContent<T> : Content
    {
        public StaticContent(T Value, Type<T> Type)
        {
            this._Value = Value;
            this._Type = Type;
        }

        public override Disposable<Visual> CreateVisual(Theme Theme)
        {
            // Get the block for the content
            Block block = this._Type.CreateBlock(Theme, this._Value);

            // Apply border and background and create control
            Border border; Color background;
            Theme.GetNodeStyle(out border, out background);
            block = block.WithBorder(border);
            block = block.WithBackground(background);
            Rectangle sizerange = new Rectangle(1.0, 1.0, 5.0, 5.0);
            return (Control)block.CreateControl(sizerange);
        }

        /// <summary>
        /// Gets the value for this content.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        /// <summary>
        /// Gets the type for this content.
        /// </summary>
        public Type<T> Type
        {
            get
            {
                return this._Type;
            }
        }

        private T _Value;
        private Type<T> _Type;
    }
}