using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A resizable block of selectable and possibly editable text and items arranged in a uniform grid.
    /// </summary>
    public class TextBlock : Block
    {
        public TextBlock()
        {
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

            // Unlink section
            prev._Next = next;
            if (next != null)
            {
                next._Previous = prev;
            }
            else
            {
                this._Last = prev;
            }

            // Fix line references
            prev.SearchBack<LineStartTextItem>(null)._NextLine = null;
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

            // Link section
            prev._Next = Section._First;
            Section._First._Previous = prev;
            Section._Last._Next = next;
            if (next != null)
            {
                next._Previous = Section._Last;
            }
            else
            {
                this._Last = Section._Last;
            }

            // Fix line references
            prev.SearchBack<LineStartTextItem>(null)._NextLine = null;
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
            /// <summary>
            /// Gets the full size of this layout.
            /// </summary>
            public Point Size
            {
                get
                {
                    TextStyle style = this.TextBlock._Style;
                    Point cellsize = style.CellSize;
                    return new Point(cellsize.X * this.Width, cellsize.Y * this.Height);
                }
            }

            /// <summary>
            /// Gets the caret for a selection at the given offset.
            /// </summary>
            public TextCaret Select(Point Offset)
            {
                TextStyle style = this.TextBlock._Style;
                Point cellsize = style.CellSize;
                int y = Math.Min(this.Height - 1, Math.Max(0, (int)(Offset.Y / cellsize.Y)));
                int offset = Math.Min(this.Width, Math.Max(0, (int)(Offset.X / cellsize.X + 0.5)));

                LineStartTextItem line = this.TextBlock._First;
                LineStartTextItem nextline;
                while (y > 0 && (nextline = line.NextLine) != null)
                {
                    line = nextline;
                    y--;
                }

                int curoffset = 0;
                TextItem cur = line;
                while (true)
                {
                    if (curoffset >= offset)
                    {
                        return new TextCaret(cur);
                    }
                    TextItem next = cur._Next;
                    if (next == null || next is LineStartTextItem)
                    {
                        return new TextCaret(cur);
                    }
                    else
                    {
                        cur = next;
                        _AppendLine(style, next, ref curoffset);
                    }
                }
            }

            public override RemoveHandler Link(InputContext Context)
            {
                RemoveHandler rh = null;

                // If there is already a selection, link the update handler
                {
                    _SelectionInfo sel = this.TextBlock._Selection;
                    if (sel != null)
                    {
                        this._RemoveUpdate = Context.RegisterUpdate(this.Update);
                    }
                }

                // Probe signal change handler for selection
                rh += Context.RegisterProbeSignalChange(delegate(Probe Probe, ProbeSignal Signal, bool Value, ref bool Handled)
                {
                    if (Signal == ProbeSignal.Primary && Value)
                    {
                        _SelectionInfo sel = this.TextBlock._Selection;
                        if (sel == null || sel.ReleaseProbe == null)
                        {
                            Point pos = Probe.Position;
                            if (new Rectangle(Point.Origin, this.Size).Occupies(pos))
                            {
                                Handled = true;

                                TextCaret caret = this.Select(pos);
                                this.TextBlock._Selection = new _SelectionInfo
                                {
                                    Probe = Probe,
                                    ReleaseProbe = Probe.Lock(),
                                    Selection = new TextSelection(caret),
                                    CaretBlinkTime = 0.0
                                };
                                if (this._RemoveUpdate == null)
                                {
                                    this._RemoveUpdate = Context.RegisterUpdate(this.Update);
                                }
                            }
                        }
                    }
                });

                // Remove selection and update events with remove handler
                rh += delegate
                {
                    _SelectionInfo sel = this.TextBlock._Selection;
                    if (sel != null)
                    {
                        if (sel.ReleaseProbe != null)
                        {
                            sel.ReleaseProbe();
                        }
                    }
                    if(this._RemoveUpdate != null)
                    {
                        this._RemoveUpdate();
                    }
                };

                return rh;
            }

            public override Figure Figure
            {
                get
                {
                    TextStyle style = this.TextBlock._Style;
                    Point cellsize = style.CellSize;
                    _SelectionInfo sel = this.TextBlock._Selection;
                    Figure fig = null;

                    // Back colors
                    TextItem cur = this.TextBlock._First;
                    int offset = 0;
                    double y = 0.0;

                    TextBackStyle backstyle = style.DefaultBackStyle;
                    SolidFigure colmask = new SolidFigure(backstyle.BackColor);
                    int colstartoffset = 0;
                    while ((cur = cur.Next) != null)
                    {
                        if (cur is LineStartTextItem)
                        {
                            fig += _BackColorStripFigure(colmask, cellsize, y, colstartoffset, offset);
                            colstartoffset = 0;
                            offset = 0;
                            y += cellsize.Y;
                            continue;
                        }
                        _AppendLine(style, cur, ref offset);
                    }
                    fig += _BackColorStripFigure(colmask, cellsize, y, colstartoffset, offset);

                    // Draw characters and foreground objects
                    TextItem prev = this.TextBlock._First;
                    offset = 0;
                    y = 0.0;
                    TextFontStyle fontstyle = style.DefaultFontStyle;
                    Figure caret = null;
                    while (true)
                    {
                        if (sel != null && sel.Selection._Primary._Previous == prev)
                        {
                            double blink = sel.CaretBlinkTime % style.CaretBlinkRate;
                            sel.CaretBlinkTime = blink;
                            if (blink < style.CaretBlinkRate * 0.5)
                            {
                                Border caretstyle = style.CaretStyle;
                                double x = cellsize.X * offset;
                                caret = new ShapeFigure(
                                    new PathShape(caretstyle.Weight, new SegmentPath(new Point(x, y), new Point(x, y + cellsize.Y))),
                                    new SolidFigure(caretstyle.Color));
                            }
                        }

                        if ((prev = cur = prev.Next) == null)
                        {
                            break;
                        }

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
                            Font font = fontstyle.Font;
                            Point size = font.GetSize(ci.Name);
                            Point off = new Point(cellsize.X * offset, y);
                            off.X += Align.AxisOffset(style.HorizontalAlignment, cellsize.X, size.X);
                            off.Y += Align.AxisOffset(style.VerticalAlignment, cellsize.Y, size.Y);
                            fig += font.GetGlyph(name).Translate(off);
                            offset++;
                            continue;
                        }

                        _AppendLine(style, cur, ref offset);
                    }

                    fig += caret;
                    return fig;
                }
            }

            /// <summary>
            /// Gets a figure for a stip containing a back color for a single line.
            /// </summary>
            private static Figure _BackColorStripFigure(SolidFigure Mask, Point CellSize, 
                double Y, int StartOffset, int EndOffset)
            {
                if (EndOffset > StartOffset && Mask.Color.A > 0.0)
                {
                    return new ShapeFigure(
                        new RectangleShape(
                            new Rectangle(
                                CellSize.X * StartOffset, Y,
                                CellSize.X * EndOffset, Y + CellSize.Y)),
                        Mask);
                }
                return null;
            }

            /// <summary>
            /// Update handler for a layout. The update handler should only be called when there is
            /// a selection.
            /// </summary>
            public void Update(double Time)
            {
                _SelectionInfo sel = this.TextBlock._Selection;
                if (sel != null)
                {
                    // Handle dragging
                    if (sel.ReleaseProbe != null)
                    {
                        Probe probe = sel.Probe;
                        if (probe[ProbeSignal.Primary])
                        {
                            Point pos = probe.Position;
                            TextCaret nsel = this.Select(pos);
                            if (sel.Selection._Primary != nsel)
                            {
                                sel.Selection._Primary = nsel;
                                sel.Selection._UpdateOrder();
                            }
                        }
                        else
                        {
                            sel.ReleaseProbe();
                            sel.ReleaseProbe = null;
                        }
                    }

                    // Handle caret blinking
                    sel.CaretBlinkTime += Time;
                }
                else
                {
                    this._RemoveUpdate();
                    this._RemoveUpdate = null;
                }
            }
           
            public TextBlock TextBlock;
            public int Width;
            public int Height;
            private RemoveHandler _RemoveUpdate;
        }

        /// <summary>
        /// Contains information for a selection of text.4
        /// </summary>
        private class _SelectionInfo
        {
            /// <summary>
            /// The probe this selection is for.
            /// </summary>
            public Probe Probe;

            /// <summary>
            /// Releases the lock on the probe for this selection. If this is null, the probe is not locked.
            /// </summary>
            public Action ReleaseProbe;

            /// <summary>
            /// The selected region.
            /// </summary>
            public TextSelection Selection;

            /// <summary>
            /// The current time, in seconds, after the start of the current blink cycle.
            /// </summary>
            public double CaretBlinkTime;
        }

        private LineStartTextItem _First;
        private TextItem _Last;
        private TextStyle _Style;
        private _SelectionInfo _Selection;
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
        /// The amount of time, in seconds, between blinks for a selection caret.
        /// </summary>
        public double CaretBlinkRate;

        /// <summary>
        /// The style for a selection caret.
        /// </summary>
        public Border CaretStyle;

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

        /// <summary>
        /// The caret for the previous distinct position before this one. If there is no such position, the
        /// same caret will be returned.
        /// </summary>
        public TextCaret Before
        {
            get
            {
                TextItem cur = this._Previous;
                while (true)
                {
                    TextItem prev = cur._Previous;
                    if (prev == null)
                    {
                        return new TextCaret(cur);
                    }
                    if (_Visible(cur))
                    {
                        return new TextCaret(prev);
                    }
                    cur = prev;
                }
            }
        }

        /// <summary>
        /// The caret for the next distinct position after this one. If there is no such position, the
        /// same caret will be returned.
        /// </summary>
        public TextCaret After
        {
            get
            {
                TextItem cur = this._Previous;
                while (true)
                {
                    TextItem next = cur._Next;
                    if (next == null)
                    {
                        return new TextCaret(cur);
                    }
                    if (_Visible(next))
                    {
                        return new TextCaret(next);
                    }
                    cur = next;
                }
            }
        }

        /// <summary>
        /// Gets if the given item has a width over 0.
        /// </summary>
        private static bool _Visible(TextItem Item)
        {
            return 
                Item is LineStartTextItem || Item is SpaceTextItem || 
                Item is CharacterTextItem || Item is IndentTextItem;
        }

        public static bool operator ==(TextCaret A, TextCaret B)
        {
            return A._Previous == B._Previous;
        }

        public static bool operator !=(TextCaret A, TextCaret B)
        {
            return A._Previous != B._Previous;
        }

        public override bool Equals(object obj)
        {
            return this._Previous == ((TextCaret)obj)._Previous;
        }

        public override int GetHashCode()
        {
            return this._Previous.GetHashCode();
        }

        internal TextItem _Previous;
    }

    /// <summary>
    /// A possible selection within a text block that can either be a single region, or a directed region of items.
    /// </summary>
    public struct TextSelection
    {
        internal TextSelection(TextCaret Caret)
        {
            this._Primary = Caret;
            this._Secondary = Caret;
            this._Order = false;
        }

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

        /// <summary>
        /// Calculates the order of the selection.
        /// </summary>
        internal bool _CalculateOrder()
        {
            TextItem cp = this._Primary._Previous;
            TextItem sp = this._Secondary._Previous;
            TextItem csp = sp;
            LineStartTextItem cl;
            LineStartTextItem sl;
            
            while ((sl = csp as LineStartTextItem) == null)
            {
                if (csp == cp)
                {
                    return false;
                }
                csp = csp._Previous;
            }

            while ((cl = cp as LineStartTextItem) == null)
            {
                cp = cp._Previous;
                if (cp == sp)
                {
                    return true;
                }
            }

            while ((cl = cl.NextLine) != null)
            {
                if (cl == sl)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the order of the selection in response to a change in either of the carets.
        /// </summary>
        internal void _UpdateOrder()
        {
            this._Order = this._CalculateOrder();
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
                if (this._NextLine != null)
                {
                    return this._NextLine;
                }
                else
                {
                    TextItem cur = this._Next;
                    while (cur != null)
                    {
                        LineStartTextItem lst = cur as LineStartTextItem;
                        if (lst != null)
                        {
                            return this._NextLine = lst;
                        }
                        cur = cur._Next;
                    }
                    return null;
                }
            }
        }

        internal LineStartTextItem _NextLine;
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