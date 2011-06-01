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

        public override Disposable<Control> CreateControl(Theme Theme)
        {
            Control space = SpaceControl.Singleton;
            Control border = new BorderControl
            {
                Inner = space,
                Border = new Border
                {
                    Color = Color.RGB(0.2, 0.2, 0.2),
                    Weight = 0.05,
                }
            };
            Control background = new BackgroundControl()
            {
                Inner = border,
                Color = Color.RGB(0.95, 0.95, 0.95)
            };
            Control size = new SizeControl
            {
                Inner = background,
                LimitSizeRange = new Rectangle(1.0, 1.0, 2.0, 2.0)
            };
            return size;
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