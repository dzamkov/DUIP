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

        }

        public TextBlock(TextSection Text, TextStyle Style)
        {
            this._Text = Text;
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
        /// Gets or sets the (pre-styled) text displayed by the text block. Note that using this property to set the text will
        /// clear the current text selection, if any exists.
        /// </summary>
        [DynamicProperty]
        public TextSection Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                this._Text = value;
            }
        }

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            TextStyle style = this._Style;
            Point cellsize = style.CellSize;

            int minwidth = (int)(SizeRange.Left / cellsize.X);
            int minheight = (int)(SizeRange.Top / cellsize.Y);

            // Calculate size
            int width, height;
            //_CalculateSize(this._First, style, out width, out height);
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
                Offset += sti.Length;
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

            public override RemoveHandler Link(Context Context)
            {
                // Update context and link information
                this._Context = Context;
                this.TextBlock._Linked = this;
                RemoveHandler rh = delegate
                {
                    this._Context = null;
                    this.TextBlock._Linked = null;
                };

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
                                this.UpdateSelection(this.TextBlock._Selection = new _SelectionInfo
                                {
                                    Probe = Probe,
                                    ReleaseProbe = Probe.Lock(),
                                    Selection = new TextSelection(caret),
                                    CaretBlinkTime = 0.0
                                });
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
                    if (this._RemoveUpdate != null)
                    {
                        this._RemoveUpdate();
                    }
                    if (this._RemoveProbeMessage != null)
                    {
                        this._RemoveProbeMessage();
                    }
                };

                return rh;
            }

            /// <summary>
            /// Informs the layout that the selection for it has been changed. This should only be called when the layout
            /// is linked.
            /// </summary>
            public void UpdateSelection(_SelectionInfo Selection)
            {
                if (this._RemoveUpdate == null && Selection != null)
                {
                    this._RemoveUpdate = this._Context.RegisterUpdate(this.Update);
                }
                else
                {
                    if (this._RemoveUpdate != null && Selection == null)
                    {
                        this._RemoveUpdate();
                        this._RemoveUpdate = null;
                    }
                }
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
                                sel.CaretBlinkTime = 0.0;
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

            /// <summary>
            /// Probe message handler for a layout.
            /// </summary>
            public void ProbeMessage(ProbeMessage Message)
            {

            }
           
            public TextBlock TextBlock;
            public int Width;
            public int Height;

            private Context _Context;
            private Action _ReleaseProbe;
            private RemoveHandler _RemoveProbeMessage;
            private RemoveHandler _RemoveUpdate;
        }

        /// <summary>
        /// Contains information for a selection of text.
        /// </summary>
        private class _SelectionInfo
        {
            /// <summary>
            /// The selected region.
            /// </summary>
            //public TextSelection Selection;

            /// <summary>
            /// The current time, in seconds, after the start of the current blink cycle.
            /// </summary>
            public double CaretBlinkTime;
        }

        private TextSection _Text;
        private TextStyle _Style;
        private _SelectionInfo _Selection;
        private _Layout _Linked;
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
    /// A section of text displayable by a text block.
    /// </summary>
    /// <remarks>Positions in a text section are given by an integer representing the index of the item directly after
    /// the position. If a position is equal to the size of the text section, the position represents the end of the text section.</remarks>
    public abstract class TextSection
    {
        public TextSection(int Size)
        {
            this.Size = Size;
        }

        /// <summary>
        /// The size of the text section.
        /// </summary>
        public readonly int Size;

        /// <summary>
        /// Inserts the given text section into this section at the given position and returns the resulting text section.
        /// </summary>
        public TextSection Insert(int Position, TextSection Section)
        {
            if (Position == 0)
                return Section + this;
            if (Position == this.Size)
                return this + Section;

            ConcatTextSection cts = this as ConcatTextSection;
            if (cts != null)
            {
                int asize = cts.A.Size;
                if (Position <= asize)
                    return cts.A.Insert(Position, Section) + cts.B;
                else
                    return cts.A + cts.B.Insert(Position, Section);
            }

            return this.Subsection(0, Position) + Section + this.Subsection(Position, this.Size);
        }

        /// <summary>
        /// Removes the items between the given positions and returns the resulting text section.
        /// </summary>
        public TextSection Remove(int Start, int End)
        {
            return this.Subsection(0, Start) + this.Subsection(End, this.Size);
        }

        /// <summary>
        /// Returns the text section for the items between the given positions.
        /// </summary>
        public TextSection Subsection(int Start, int End)
        {
            if (Start == 0 && End == this.Size)
                return this;
            if (Start == End)
                return NullTextSection.Instance;

            ConcatTextSection cts = this as ConcatTextSection;
            if (cts != null)
            {
                int asize = cts.A.Size;
                if (Start >= asize)
                    return cts.B.Subsection(Start - asize, End - asize);
                if (End <= asize)
                    return cts.A.Subsection(Start, End);
                return cts.A.Subsection(Start, asize) + cts.B.Subsection(0, End - asize);
            }

            throw new NotImplementedException();
        }

        public static TextSection operator +(TextSection A, TextSection B)
        {
            if (A == NullTextSection.Instance)
                return B;
            if (B == NullTextSection.Instance)
                return A;
            return new ConcatTextSection(A, B);
        }

        public static implicit operator TextSection(string Source)
        {
            TextSection sect = NullTextSection.Instance;
            for (int t = 0; t < Source.Length; t++)
            {
                char c = Source[t];
                switch (c)
                {
                    case '\b':
                    case '\r':
                        break;
                    case '\n':
                        sect += TextItem.LineStart;
                        break;
                    case '\t':
                        sect += TextItem.Indent;
                        break;
                    case ' ':
                        sect += TextItem.Space(1);
                        break;
                    default:
                        sect += TextItem.Character(c);
                        break;
                }
            }
            return sect;
        }
    }

    /// <summary>
    /// A text section containing no items.
    /// </summary>
    public sealed class NullTextSection : TextSection
    {
        private NullTextSection()
            : base(0)
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly NullTextSection Instance = new NullTextSection();
    }

    /// <summary>
    /// A text section created by concatenating two text sections.
    /// </summary>
    public sealed class ConcatTextSection : TextSection
    {
        public ConcatTextSection(TextSection A, TextSection B) :
            base(A.Size + B.Size)
        {
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Gets the first of the components of this text section.
        /// </summary>
        public TextSection A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets the second of the components of this text section.
        /// </summary>
        public TextSection B
        {
            get
            {
                return this._B;
            }
        }

        private TextSection _A;
        private TextSection _B;
    }

    /// <summary>
    /// A text section for a single indivisible item.
    /// </summary>
    public abstract class TextItem : TextSection
    {
        public TextItem()
            : base(1)
        {

        }

        /// <summary>
        /// Gets a line start text item.
        /// </summary>
        public static LineStartTextItem LineStart
        {
            get
            {
                return LineStartTextItem.Instance;
            }
        }

        /// <summary>
        /// Gets an indent text item.
        /// </summary>
        public static IndentTextItem Indent
        {
            get
            {
                return IndentTextItem.Instance;
            }
        }

        /// <summary>
        /// Gets a space text item of a certain length.
        /// </summary>
        public static SpaceTextItem Space(int Length)
        {
            if (Length < _SpaceCacheSize)
            {
                SpaceTextItem item = _SpaceCache[Length];
                if (item == null)
                    _SpaceCache[Length] = item = new SpaceTextItem(Length);
                return item;
            }
            return new SpaceTextItem(Length);
        }
        private const int _SpaceCacheSize = 16;
        private static readonly SpaceTextItem[] _SpaceCache = new SpaceTextItem[_SpaceCacheSize];

        /// <summary>
        /// Gets a character text item for a character of a certain name.
        /// </summary>
        public static CharacterTextItem Character(char Name)
        {
            int ind = (int)Name - _CharacterCacheStart;
            if (ind >= 0 && ind < _CharacterCacheSize)
            {
                CharacterTextItem item = _CharacterCache[ind];
                if (item == null)
                    _CharacterCache[ind] = item = new CharacterTextItem(Name);
                return item;
            }
            return new CharacterTextItem(Name);
        }
        private const int _CharacterCacheStart = 33;
        private const int _CharacterCacheSize = 93;
        private static readonly CharacterTextItem[] _CharacterCache = new CharacterTextItem[_CharacterCacheSize];
    }

    /// <summary>
    /// A text item that signals the start of a line.
    /// </summary>
    public sealed class LineStartTextItem : TextItem
    {
        private LineStartTextItem()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly LineStartTextItem Instance = new LineStartTextItem();
    }

    /// <summary>
    /// A text item that marks a variable-length indentation.
    /// </summary>
    public sealed class IndentTextItem : TextItem
    {
        private IndentTextItem()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly IndentTextItem Instance = new IndentTextItem();
    }

    /// <summary>
    /// A text item that signals a certain amount of space.
    /// </summary>
    public sealed class SpaceTextItem : TextItem
    {
        public SpaceTextItem(int Length)
        {
            this.Length = Length;
        }

        /// <summary>
        /// The length of the space text item.
        /// </summary>
        public readonly int Length;
    }

    /// <summary>
    /// A text item for a certain character.
    /// </summary>
    public sealed class CharacterTextItem : TextItem
    {
        public CharacterTextItem(char Name)
        {
            this.Name = Name;
        }

        /// <summary>
        /// The name of the character for the text item.
        /// </summary>
        public readonly char Name;
    }
}