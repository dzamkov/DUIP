using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A resizable block of selectable and possibly editable text and items arranged in a uniform grid.
    /// </summary>
    public class TextBlock : Block
    {
        public TextBlock()
        {
            this._Layouts = new Registry<_Layout>();
            LineStartTextItem first = new LineStartTextItem();
            this._First = first;
            this._Last = first;
        }

        public TextBlock(TextStyle Style)
            : this()
        {
            this._Style = Style;
        }

        /// <summary>
        /// Gets or sets the style of the text block.
        /// </summary>
        [StaticProperty]
        public TextStyle Style
        {
            get
            {
                return this._Style;
            }
            set
            {
                this._Style = value;
            }
        }

        /// <summary>
        /// Gets a caret for the beginning of the text.
        /// </summary>
        public TextCaret Start
        {
            get
            {
                return new TextCaret(this._First);
            }
        }

        /// <summary>
        /// Gets a caret for the end of the text.
        /// </summary>
        public TextCaret End
        {
            get
            {
                return new TextCaret(this._Last);
            }
        }

        /// <summary>
        /// Removes all items between the two carets.
        /// </summary>
        public void Remove(TextCaret Start, TextCaret End)
        {
            TextItem prev = Start._Previous;
            TextItem next = End._Previous._Next;
            prev._Next = next;
            if (next != null)
            {
                next._Previous = prev;
            }
            else
            {
                this._Last = prev;
            }
        }

        /// <summary>
        /// Removes all items between the two carets and returns them as a text section.
        /// </summary>
        public TextSection Cut(TextCaret Start, TextCaret End)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the given section at the given position. The section may no longer be used after this call.
        /// </summary>
        public void Insert(TextCaret Position, TextSection Section)
        {
            TextItem prev = Position._Previous;
            TextItem next = prev._Next;

            prev._Next = Section._First;
            if (next != null)
            {
                next._Previous = Section._Last;
            }
            else
            {
                this._Last = Section._Last;
            }

            foreach (_Layout layout in this._Layouts)
            {
                layout.Invalidate();
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            TextStyle style = this._Style;
            Point cellsize = style.CellSize;

            int minwidth = (int)(SizeRange.Left / cellsize.X);
            int minheight = (int)(SizeRange.Top / cellsize.Y);

            // Calculate size
            int width, height;
            _CalculateSize(this._First, style, out width, out height);
            width = Math.Max(width, minwidth);
            height = Math.Max(height, minheight);

            Size = new Point(width * cellsize.X, height * cellsize.Y);
            Size.X = Math.Min(Size.X, SizeRange.Right);
            Size.Y = Math.Min(Size.Y, SizeRange.Bottom);
            _Layout layout = new _Layout
            {
                TextPad = this
            };
            this._Layouts.Register(layout);
            return layout;
        }

        /// <summary>
        /// Calculates the size (in cells) of text block starting with the given item and using the given style.
        /// </summary>
        private static void _CalculateSize(LineStartTextItem First, TextStyle Style, out int Width, out int Height)
        {
            Width = 0;
            Height = 1;
            int offset = 0;
            TextItem cur = First;
            while ((cur = cur.Next) != null)
            {
                if (cur is LineStartTextItem)
                {
                    Height++;
                    Width = Math.Max(offset, Width);
                    offset = 0;
                }
                else
                {
                    _AppendLine(Style, cur, ref offset);
                }
            }
            Width = Math.Max(offset, Width);
        }

        /// <summary>
        /// Determines the width of an item when placed at the given offset from the left edge of a text block and adds it to
        /// the offset.
        /// </summary>
        private static void _AppendLine(TextStyle Style, TextItem Item, ref int Offset)
        {
            if (Item is IndentTextItem)
            {
                Style.Indent(ref Offset);
                return;
            }

            SpaceTextItem sti = Item as SpaceTextItem;
            if (sti != null)
            {
                Offset += sti.Width;
                return;
            }

            if (Item is CharacterTextItem)
            {
                Offset++;
                return;
            }
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes)
            {

            }

            public override void Render(RenderContext Context)
            {
                TextStyle style = this.TextPad._Style;
                Point cellsize = style.CellSize;

                TextItem cur = this.TextPad._First;
                int offset = 0;
                double y = 0.0;

                // Draw characters and foreground objects
                TextFontStyle fontstyle = style.DefaultFontStyle;
                Font.MultiDrawer fontdrawer = new Font.MultiDrawer();
                fontdrawer.Select(Context, fontstyle.Font);
                while ((cur = cur.Next) != null)
                {
                    if (cur is LineStartTextItem)
                    {
                        offset = 0;
                        y += cellsize.Y;
                        continue;
                    }

                    if (cur is IndentTextItem)
                    {
                        style.Indent(ref offset);
                        continue;
                    }

                    SpaceTextItem sti = cur as SpaceTextItem;
                    if (sti != null)
                    {
                        offset += sti.Width;
                         continue;
                    }

                    CharacterTextItem ci = cur as CharacterTextItem;
                    if (ci != null)
                    {
                        char name = ci.Name;
                        Font font = fontdrawer.Font;
                        Point size = font.GetSize(ci.Name);
                        Point off = new Point(cellsize.X * offset, y);
                        off.X += Align.AxisOffset(style.HorizontalAlignment, cellsize.X, size.X);
                        off.Y += Align.AxisOffset(style.VerticalAlignment, cellsize.Y, size.Y);

                        fontdrawer.Draw(Context, name, off);
                        offset++;
                    }
                }
                fontdrawer.Flush(Context);
            }

            /// <summary>
            /// Marks the layout as invalid.
            /// </summary>
            public void Invalidate()
            {
                if (this.Invalidated != null)
                {
                    this.Invalidated();
                }
            }

            public override event Action Invalidated;

            public TextBlock TextPad;
        }

        private Registry<_Layout> _Layouts;
        private LineStartTextItem _First;
        private TextItem _Last;
        private TextStyle _Style;
    }

    /// <summary>
    /// Contains styling information for a text block.
    /// </summary>
    public class TextStyle
    {
        /// <summary>
        /// The horizontal alignment of items within their cells.
        /// </summary>
        public Alignment HorizontalAlignment;

        /// <summary>
        /// The vertical alignment of items within their cells.
        /// </summary>
        public Alignment VerticalAlignment;

        /// <summary>
        /// The default font style for items in the text block.
        /// </summary>
        public TextFontStyle DefaultFontStyle;

        /// <summary>
        /// The default background style for items in the text block.
        /// </summary>
        public TextBackStyle DefaultBackStyle;

        /// <summary>
        /// The size of a cell in the text block.
        /// </summary>
        public Point CellSize;

        /// <summary>
        /// The size of an indent span.
        /// </summary>
        public int IndentSize;

        /// <summary>
        /// Updates the given offset from the left edge of a text pad with an indentation.
        /// </summary>
        public void Indent(ref int Offset)
        {
            Offset = ((Offset / this.IndentSize) + 1) * this.IndentSize;
        }

        /// <summary>
        /// Gets the full size of a text pad with the given amount width and height in cells using this style.
        /// </summary>
        public Point GetSize(int Width, int Height)
        {
            return new Point(this.CellSize.X * Width, this.CellSize.Y * Height);
        }
    }

    /// <summary>
    /// Contains styling information for the font used for a section of text in a text block.
    /// </summary>
    public class TextFontStyle
    {
        /// <summary>
        /// The default font for text.
        /// </summary>
        public Font Font;

        /// <summary>
        /// The font for selected text.
        /// </summary>
        public Font SelectedFont;
    }

    /// <summary>
    /// Contains styling information for the back color of a section of items in a text block.
    /// </summary>
    public class TextBackStyle
    {
        /// <summary>
        /// The default back color of items.
        /// </summary>
        public Color BackColor;

        /// <summary>
        /// The back color for selected items.
        /// </summary>
        public Color SelectedBackColor;
    }

    /// <summary>
    /// A position of a space between items within a text block. If the contents of the text block change, the
    /// caret will remain in the same position relative to surronding items.
    /// </summary>
    public struct TextCaret
    {
        internal TextCaret(TextItem Previous)
        {
            this._Previous = Previous;
        }

        /// <summary>
        /// The item immediately before this caret.
        /// </summary>
        public TextItem Previous
        {
            get
            {
                return this._Previous;
            }
        }

        /// <summary>
        /// The item immediately after this caret.
        /// </summary>
        public TextItem Next
        {
            get
            {
                return this._Previous._Next;
            }
        }

        internal TextItem _Previous;
    }

    /// <summary>
    /// A unique section of text items. Note that a text section can not be created with items that
    /// are in use by a text block.
    /// </summary>
    public struct TextSection
    {
        public TextSection(TextItem First, TextItem Last)
        {
            this._First = First;
            this._Last = Last;
        }

        public TextSection(TextItem Item)
        {
            this._First = Item;
            this._Last = Item;
        }

        /// <summary>
        /// Gets the first item in this section.
        /// </summary>
        public TextItem First
        {
            get
            {
                return this._First;
            }
        }

        /// <summary>
        /// Gets the last item in this section.
        /// </summary>
        public TextItem Last
        {
            get
            {
                return this._Last;
            }
        }

        /// <summary>
        /// Creates a text section for the given text, using spaces, indents, and line breaks where needed.
        /// </summary>
        public static TextSection Create(string Text)
        {
            if (Text.Length > 0)
            {
                TextItem prev = TextItem.Create(Text[0]);
                TextItem first = prev;
                for (int t = 1; t < Text.Length; t++)
                {
                    TextItem cur = TextItem.Create(Text[t]);
                    if (cur != null)
                    {
                        cur._Previous = prev;
                        prev._Next = cur;
                        prev = cur;
                    }
                }
                TextItem last = prev;
                return new TextSection(first, last);
            }
            else
            {
                return new TextSection();
            }
        }

        public static TextSection operator +(TextSection First, TextSection Last)
        {
            return new TextSection(First.First, Last.Last);
        }

        internal TextItem _First;
        internal TextItem _Last;
    }

    /// <summary>
    /// A unique item within a text block.
    /// </summary>
    public class TextItem
    {
        /// <summary>
        /// Gets the item immediately before this item.
        /// </summary>
        public TextItem Previous
        {
            get
            {
                return this._Previous;
            }
        }

        /// <summary>
        /// Gets the item immediately after this item.
        /// </summary>
        public TextItem Next
        {
            get
            {
                return this._Next;
            }
        }

        /// <summary>
        /// Creates a text item for the given character, using spaces, indents, and line breaks where needed. Returns
        /// null if the character should be ignored.
        /// </summary>
        public static TextItem Create(char Character)
        {
            switch (Character)
            {
                case ' ':
                    return new SpaceTextItem { Width = 1 };
                case '\r':
                    return null;
                case '\n':
                    return new LineStartTextItem();
                case '\t':
                    return new IndentTextItem();
                default:
                    return new CharacterTextItem { Name = Character };
            }
        }

        internal TextItem _Previous;
        internal TextItem _Next;
    }

    /// <summary>
    /// A text item that marks the start of a line.
    /// </summary>
    public class LineStartTextItem : TextItem
    {

    }

    /// <summary>
    /// A text item that marks an indentation.
    /// </summary>
    public class IndentTextItem : TextItem
    {

    }

    /// <summary>
    /// A text item that displays a single character.
    /// </summary>
    public class CharacterTextItem : TextItem
    {
        /// <summary>
        /// The name of the character to display.
        /// </summary>
        public char Name;
    }

    /// <summary>
    /// A text item that displays a space with a certain width.
    /// </summary>
    public class SpaceTextItem : TextItem
    {
        /// <summary>
        /// The width of the space to display.
        /// </summary>
        public int Width;
    }
}