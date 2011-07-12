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

        public override Layout CreateLayout(InputContext Context, Rectangle SizeRange, out Point Size)
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
                TextBlock = this,
                Width = width,
                Height = height
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
            public override void Render(RenderContext Context)
            {
                TextStyle style = this.TextBlock._Style;
                Point cellsize = style.CellSize;

                // Draw back colors
                TextItem cur = this.TextBlock._First;
                int offset = 0;
                double y = 0.0;

                TextBackStyle backstyle = style.DefaultBackStyle;
                int colstartoffset = 0;
                using (Context.DrawQuads())
                {
                    while ((cur = cur.Next) != null)
                    {
                        if (cur is LineStartTextItem)
                        {
                            _OutputBackColorStrip(Context, backstyle, cellsize, y, colstartoffset, offset);
                            colstartoffset = 0;
                            offset = 0;
                            y += cellsize.Y;
                            continue;
                        }

                        _AppendLine(style, cur, ref offset);
                    }
                    _OutputBackColorStrip(Context, backstyle, cellsize, y, colstartoffset, offset);
                }

                // Draw characters and foreground objects
                cur = this.TextBlock._First;
                offset = 0;
                y = 0.0;
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
                        continue;
                    }

                    _AppendLine(style, cur, ref offset);
                }
                fontdrawer.Flush(Context);
            }

            /// <summary>
            /// Outputs a quad for a section of a back color applied on a single line.
            /// </summary>
            private static void _OutputBackColorStrip(
                RenderContext Context, TextBackStyle Style, Point CellSize, 
                double Y, int StartOffset, int EndOffset)
            {
                if (EndOffset > StartOffset)
                {
                    Color color = Style.BackColor;
                    if (color.A > 0.0)
                    {
                        Context.SetColor(color);
                        Context.OutputQuad(
                            new Rectangle(
                                CellSize.X * StartOffset, Y, 
                                CellSize.X * EndOffset, Y + CellSize.Y));
                    }
                }
            }

            /// <summary>
            /// Marks the layout as invalid.
            /// </summary>
            public void Invalidate()
            {
                if (this._Invalidate != null)
                {
                    this._Invalidate();
                }
            }
            private Action _Invalidate;

            public override RemoveHandler RegisterInvalidate(Action Callback)
            {
                this._Invalidate += Callback;
                return delegate { this._Invalidate -= Callback; };
            }

           
            public TextBlock TextBlock;
            public int Width;
            public int Height;
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
    /// A possible selection within a text block that can either be a single region, or a directed region of items.
    /// </summary>
    public struct TextSelection
    {
        /// <summary>
        /// Gets the caret for the primary selection point. This is where the cursor should appear.
        /// </summary>
        public TextCaret Primary
        {
            get
            {
                return this._Primary;
            }
        }

        /// <summary>
        /// Gets the caret for the secondary selection point. This shows the extent of the selection. If this is the same as the primary
        /// selection point, then the selection is only at one point.
        /// </summary>
        public TextCaret Secondary
        {
            get
            {
                return this._Secondary;
            }
        }

        /// <summary>
        /// Gets the start caret of the selected region.
        /// </summary>
        public TextCaret Start
        {
            get
            {
                return this._Order ? this._Secondary : this._Primary;
            }
        }

        /// <summary>
        /// Gets the end caret of the selected region.
        /// </summary>
        public TextCaret End
        {
            get
            {
                return this._Order ? this._Primary : this._Secondary;
            }
        }

        /// <summary>
        /// Gets wether the primary caret is after the secondary caret.
        /// </summary>
        public bool Order
        {
            get
            {
                return this._Order;
            }
        }

        internal TextCaret _Primary;
        internal TextCaret _Secondary;
        internal bool _Order;
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
        /// Gets the line start item for the line this item is on.
        /// </summary>
        public LineStartTextItem Line
        {
            get
            {
                return this.SearchBack<LineStartTextItem>(null);
            }
        }

        /// <summary>
        /// Searches for an item of the given type starting with this item and going backwards. If no item is found before reaching the given stop item, null is returned.
        /// </summary>
        public T SearchBack<T>(TextItem Stop)
            where T : TextItem
        {
            TextItem cur = this;
            while (true)
            {
                if (cur == Stop)
                {
                    return null;
                }

                T test = cur as T;
                if (test != null)
                {
                    return test;
                }

                cur = cur._Previous;
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
        /// <summary>
        /// Gets the next line start item following this one, or null if there isn't one.
        /// </summary>
        public LineStartTextItem NextLine
        {
            get
            {
                if (this._NextLineStart != null)
                {
                    return this._NextLineStart;
                }
                else
                {
                    TextItem cur = this._Next;
                    while (cur != null)
                    {
                        LineStartTextItem lst = cur as LineStartTextItem;
                        if (lst != null)
                        {
                            return this._NextLineStart = lst;
                        }
                        cur = cur._Next;
                    }
                    return null;
                }
            }
        }

        internal LineStartTextItem _NextLineStart;
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