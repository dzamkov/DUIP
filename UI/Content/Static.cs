using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A content representation of a static instance of a certain type.
    /// </summary>
    public class StaticContent<T> : Content
    {
        public StaticContent(T Value, Type<T> Type)
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