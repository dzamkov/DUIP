using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A content representation of a static instance of a certain type.
    /// </summary>
    public class StaticContent : Content
    {
        public StaticContent(Type Type, object Value)
        {
            this._Value = Value;
            this._Type = Type;
        }

        public override Disposable<Block> CreateBlock(Theme Theme)
        {
            Border border; Color background;
            Theme.GetNodeStyle(out border, out background);
            return this._Type.CreateBlock(this._Value, Theme).WithBorder(border).WithBackground(background);
        }

        /// <summary>
        /// Gets the value for this content.
        /// </summary>
        public object Value
        {
            get
            {
                return this._Value;
            }
        }

        /// <summary>
        /// Gets the type for this content.
        /// </summary>
        public Type Type
        {
            get
            {
                return this._Type;
            }
        }

        private object _Value;
        private Type _Type;
    }
}