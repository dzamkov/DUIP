using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A resizable block of editable text and items arranged in a uniform grid.
    /// </summary>
    public class TextPad : Block
    {
        public TextPad()
        {
            this._Layouts = new Registry<_Layout>();
            _LineStartItem first = new _LineStartItem();
            this._First = first;
            this._Last = first;
        }

        public TextPad(TextPadStyle Style)
            : this()
        {
            this._Style = Style;
        }

        /// <summary>
        /// Gets or sets the style of the text pad.
        /// </summary>
        [StaticProperty]
        public TextPadStyle Style
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
        /// Represents a possible caret position (in relation to items) in a text pad. If the contents of the text pad change,
        /// the caret will keep its position relative to the items around it.
        /// </summary>
        public struct Caret
        {
            internal Caret(_Item Before)
            {
                this.Before = Before;
            }

            /// <summary>
            /// The item before this caret.
            /// </summary>
            internal _Item Before; 
        }

        /// <summary>
        /// A style for text in a text pad.
        /// </summary>
        public class TextStyle
        {
            /// <summary>
            /// The normal font for the text.
            /// </summary>
            public Font Font;

            /// <summary>
            /// The font for the text when selected.
            /// </summary>
            public Font SelectedFont;

            /// <summary>
            /// The background color for the text.
            /// </summary>
            public Color BackColor;

            /// <summary>
            /// The background color for the text when selected.
            /// </summary>
            public Color SelectedBackColor;
        }

        /// <summary>
        /// Gets a caret for the beginning of the text.
        /// </summary>
        public Caret Start
        {
            get
            {
                return new Caret(this._First);
            }
        }

        /// <summary>
        /// Gets a caret for the end of the text.
        /// </summary>
        public Caret End
        {
            get
            {
                return new Caret(this._Last);
            }
        }

        /// <summary>
        /// Gets a position relative to the given one. A positive amount indicates that the returned position should be ahead of the
        /// given position by the given amount of items. A negative amout indicates that the returned position should be behind the
        /// given position by the given amount of items.
        /// </summary>
        public Caret Advance(Caret Position, int Amount)
        {
            while (Amount > 0)
            {
                _Item next = Position.Before.Next;
                if (next == null)
                {
                    return Position;
                }
                Position.Before = next;
                Amount--;
            }
            while (Amount < 0)
            {
                _Item prev = Position.Before.Previous;
                if (prev == null)
                {
                    return Position;
                }
                Position.Before = prev;
                Amount++;
            }
            return Position;
        }

        /// <summary>
        /// Gets the positions for the start and end of the line containing given caret.
        /// </summary>
        public void GetLine(Caret Position, out Caret LineStart, out Caret LineEnd)
        {
            LineStart = Position;
            while (!(LineStart.Before is _LineStartItem))
            {
                LineStart.Before = LineStart.Before.Previous;
            }

            LineEnd = Position;
            _Item next;
            while (!((next = LineEnd.Before.Next) is _LineStartItem))
            {
                LineEnd.Before = next;
            }
        }

        /// <summary>
        /// Removes all items between the two carets.
        /// </summary>
        public void Remove(Caret Start, Caret End)
        {
            End.Before = End.Before.Next;
            Start.Before.Next = End.Before;
            End.Before.Previous = Start.Before;
        }

        /// <summary>
        /// Inserts a line break at the given position.
        /// </summary>
        public void InsertLineBreak(Caret Position)
        {
            _Item prev = Position.Before;
            _Item next = prev.Next;
            _LineStartItem cur = new _LineStartItem();
            prev.Next = cur;
            cur.Previous = prev;
            if (next != null)
            {
                next.Previous = cur;
                cur.Next = next;
            }

            foreach (_Layout layout in this._Layouts)
            {
                layout.Invalidate();
            }
        }

        /// <summary>
        /// Inserts an indentation at the given position.
        /// </summary>
        public void InsertIndent(Caret Position)
        {
            _Item prev = Position.Before;
            _Item next = prev.Next;
            _IndentItem cur = new _IndentItem();
            prev.Next = cur;
            cur.Previous = prev;
            if (next != null)
            {
                next.Previous = cur;
                cur.Next = next;
            }

            foreach (_Layout layout in this._Layouts)
            {
                layout.Invalidate();
            }
        }

        /// <summary>
        /// Inserts text at the given position.
        /// </summary>
        public void InsertText(Caret Position, string Text, TextStyle Style)
        {
            _Item prev = Position.Before;
            _Item next = prev.Next;
            foreach (char c in Text)
            {
                _CharacterItem cur = new _CharacterItem
                {
                    Name = c,
                    Style = Style,
                    Previous = prev
                };
                prev.Next = cur;
                prev = cur;
            }
            if (next != null)
            {
                next.Previous = prev;
                prev.Next = next;
            }

            foreach (_Layout layout in this._Layouts)
            {
                layout.Invalidate();
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            TextPadStyle style = this._Style;
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
            return new _Layout
            {
                TextPad = this
            };
        }

        /// <summary>
        /// Calculates the size (in cells) of text pad starting with the given item and using the given style.
        /// </summary>
        private static void _CalculateSize(_LineStartItem First, TextPadStyle Style, out int Width, out int Height)
        {
            Width = 0;
            Height = 1;
            int offset = 0;
            _Item cur = First;
            while ((cur = cur.Next) != null)
            {
                if (cur is _LineStartItem)
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
        /// Determines the width of an item when placed at the given offset from the left edge of a text pad and adds it to
        /// the offset.
        /// </summary>
        private static void _AppendLine(TextPadStyle Style, _Item Item, ref int Offset)
        {
            if (Item is _IndentItem)
            {
                Style.Indent(ref Offset);
                return;
            }
            if (Item is _CharacterItem)
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
                TextPadStyle style = this.TextPad._Style;
                Point cellsize = style.CellSize;

                _Item cur = this.TextPad._First;
                int offset = 0;
                double y = 0.0;

                // Draw characters and foreground objects
                Font.MultiDrawer fontdrawer = new Font.MultiDrawer();
                while ((cur = cur.Next) != null)
                {
                    if (cur is _LineStartItem)
                    {
                        offset = 0;
                        y += cellsize.Y;
                        continue;
                    }
                    if (cur is _IndentItem)
                    {
                        style.Indent(ref offset);
                        continue;
                    }
                    _CharacterItem ci = cur as _CharacterItem;
                    if (ci != null)
                    {
                        char name = ci.Name;
                        Font font = ci.Style.Font;
                        Point size = font.GetSize(ci.Name);
                        Point off = new Point(cellsize.X * offset, y);
                        off.X += Align.AxisOffset(style.HorizontalAlignment, cellsize.X, size.X);
                        off.Y += Align.AxisOffset(style.VerticalAlignment, cellsize.Y, size.Y);

                        fontdrawer.Select(Context, font);
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

            public TextPad TextPad;
        }

        /// <summary>
        /// An item within the text pad.
        /// </summary>
        internal class _Item
        {
            public _Item Next;
            public _Item Previous;

            /// <summary>
            /// Gets the line start for the line this item is in.
            /// </summary>
            public _LineStartItem Line
            {
                get
                {
                    _Item cur = this;
                    _LineStartItem tar;
                    while ((tar = cur as _LineStartItem) == null)
                    {
                        cur = cur.Previous;
                    }
                    return tar;
                }
            }
        }

        /// <summary>
        /// An item that marks the start of a line.
        /// </summary>
        internal class _LineStartItem : _Item
        {

        }

        /// <summary>
        /// An item that marks an indentation.
        /// </summary>
        private class _IndentItem : _Item
        {

        }

        /// <summary>
        /// An item that displays a character.
        /// </summary>
        private class _CharacterItem : _Item
        {
            public char Name;
            public TextStyle Style;
        }

        

        private Registry<_Layout> _Layouts;
        private _LineStartItem _First;
        private _Item _Last;
        private TextPadStyle _Style;
    }

    /// <summary>
    /// Contains styling information for a textpad.
    /// </summary>
    public class TextPadStyle
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
        /// The size of a cell in the text pad.
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
}