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
            Control content = new FlowControl
            {
                FitMode = FlowFitMode.Compact,
                Style = new FlowStyle
                {
                    Direction = FlowDirection.RightDown,
                    Justification = FlowJustification.Justify,
                    WrapMode = FlowWrapMode.Greedy,
                    LineAlignment = Alignment.Up,
                    LineSize = 0.0,
                    LineSpacing = 0.0
                },
                Items = FlowItem.CreateText("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam tincidunt, lectus ut aliquet adipiscing, felis elit dapibus neque, ac venenatis neque nunc rutrum justo. Ut eu mi eu orci gravida venenatis vitae id nibh. In hac habitasse platea dictumst. Etiam auctor laoreet turpis id adipiscing. Sed ultricies, elit vel malesuada ornare, ligula mauris sagittis massa, sit amet suscipit metus diam eget mauris. Duis vitae risus leo, sed venenatis mauris. Donec magna orci, vehicula nec iaculis sit amet, rutrum eget elit. Vestibulum et leo at massa luctus condimentum eget vel odio. In arcu lorem, aliquam at blandit eget, sodales eget massa. Duis nisl erat, ornare at cursus vitae, molestie ut diam. Integer massa justo, fringilla tempus placerat at, rhoncus vitae orci. Proin laoreet lectus in enim commodo sed adipiscing diam auctor. Vestibulum a magna neque, nec pellentesque leo. Curabitur ornare, quam eu sodales semper, arcu lorem rhoncus mauris, ac porta odio arcu ut elit. Mauris interdum tristique odio non sagittis. Curabitur porta erat in mauris gravida semper. Donec magna lorem, aliquet vel volutpat sit amet, vulputate id augue. Curabitur porta eleifend velit, sed sagittis nulla mattis ut. Duis eget dignissim dui.", Theme.GetFont(FontPurpose.General), 0.01)
            }.WithSize(new Point(2.0, 2.0));

            Border border; Color background;
            Theme.GetNodeStyle(out border, out background);
            return content.WithPad(Theme.TextPadding).WithBorder(border).WithBackground(background);
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