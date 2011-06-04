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
            FlowStyle style = new FlowStyle
            {
                Direction = FlowDirection.RightDown,
                Justification = FlowJustification.Ragged,
                WrapMode = FlowWrap.Greedy,
                LineAlignment = Alignment.Up,
                LineSize = 0.0,
                LineSpacing = 0.0
            };

            Func<string, Control> text = delegate(string Text)
            {
                return new FlowControl
                {
                    AspectRatio = double.PositiveInfinity,
                    Style = style,
                    Items = FlowItem.CreateText(Text, Theme.GetFont(FontPurpose.General), 0.01)
                }.WithPad(Theme.TextPadding);
            };

            GridControl grid = new GridControl(8, 16);
            grid.Seperator = new Border(0.02, Color.RGB(0.2, 0.2, 0.2));
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    grid[r, c] = text(r.ToString() + " * " + c.ToString() + " = " + (r * c).ToString());
                }
            }

            Border border; Color background;
            Theme.GetNodeStyle(out border, out background);
            return grid.WithBorder(border).WithBackground(background);
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