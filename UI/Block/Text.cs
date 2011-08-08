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
        public TextBlock(TextSection Text, TextBlockStyle Style)
        {
            this.Style = Style;
            this._Text = Text;
            this.Update(ref this._Text);
        }
       
        /// <summary>
        /// The style used by the text block.
        /// </summary>
        public readonly TextBlockStyle Style;

        /// <summary>
        /// Gets the stored text for the text block.
        /// </summary>
        public TextSection Text
        {
            get
            {
                return this._Text;
            }
        }

        /// <summary>
        /// Gets the displayed text for the text block. The displayed text must have the same items as the stored text, but may have different styling
        /// information.
        /// </summary>
        public virtual TextSection DisplayedText
        {
            get
            {
                return this._Text;
            }
        }

        /// <summary>
        /// Indicates wether the contents of this text block may be edited by the user.
        /// </summary>
        protected readonly bool Editable = true;

        /// <summary>
        /// Called when the text of text block is changed. This method may modify the styling and representation (but not the contents) of the stored text
        /// before returning.
        /// </summary>
        protected virtual void Update(ref TextSection Text)
        {

        }

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            TextBlockStyle style = this.Style;
            Point cellsize = style.CellSize;

            int minwidth = (int)(SizeRange.Left / cellsize.X);
            int minheight = (int)(SizeRange.Top / cellsize.Y);

            // Calculate width, height and line indices
            List<int> lineindices = new List<int>();
            int offset = 0;
            int width = minwidth;
            lineindices.Add(0); // Initial line, not explicitly indicated, but still deserves an index
            _Measure(style, this._Text, 0, lineindices, ref offset, ref width);
            int height = Math.Max(lineindices.Count, minheight);

            // Calculate actual size
            Size = new Point(width * cellsize.X, height * cellsize.Y);
            Size.X = Math.Min(Size.X, SizeRange.Right);
            Size.Y = Math.Min(Size.Y, SizeRange.Bottom);
            _Layout layout = new _Layout
            {
                TextBlock = this,
                Width = width,
                Height = height,
                LineIndices = lineindices
            };
            return layout;
        }

        /// <summary>
        /// Determines the width of an item when placed at the given offset from the left edge of a text block and adds it to
        /// the offset.
        /// </summary>
        private static void _AppendLine(TextBlockStyle Style, TextItem Item, ref int Offset)
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

        /// <summary>
        /// Performs initial measurements on a text section to be used in a layout.
        /// </summary>
        /// <param name="LineIndices">A list to which the indices of line start items should be appened.</param>
        /// <param name="Index">The index of the first item in the text section.</param>
        /// <param name="Offset">The offset of the start of text section from the beginning of the current line. This will be changed to the
        /// offset of the end of the text section from the beginning of the latest line.</param>
        /// <param name="Width">The lower bound on the line width needed to contain the text section.</param>
        private static void _Measure(TextBlockStyle Style, TextSection Section, int Index, List<int> LineIndices, ref int Offset, ref int Width)
        {
            TextItem ti = Section as TextItem;
            if (ti != null)
            {
                if (ti == LineStartTextItem.Instance)
                {
                    Offset = 0;
                    LineIndices.Add(Index);
                    return;
                }

                _AppendLine(Style, ti, ref Offset);
                Width = Math.Max(Width, Offset);
                return;
            }

            ConcatTextSection cts = Section as ConcatTextSection;
            if (cts != null)
            {
                _Measure(Style, cts.A, Index, LineIndices, ref Offset, ref Width);
                _Measure(Style, cts.B, Index + cts.A.Size, LineIndices, ref Offset, ref Width);
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
                    TextBlockStyle style = this.TextBlock.Style;
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
                        throw new NotImplementedException();
                    }
                });

                // Clean up context with remove handler
                rh += delegate
                {
                    if (this._ReleaseProbe != null)
                    {
                        this._ReleaseProbe();
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


            public override Figure Figure
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Update handler for a layout. The update handler should only be called when there is
            /// a selection.
            /// </summary>
            public void Update(double Time)
            {
                _SelectionInfo sel = this.TextBlock._Selection;
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
            public List<int> LineIndices;

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
        private _SelectionInfo _Selection;
        private _Layout _Linked;
    }

    /// <summary>
    /// Contains styling information for a text block.
    /// </summary>
    public class TextBlockStyle
    {
        public TextBlockStyle(
            Point CellSize,
            Alignment HorizontalAlignment,
            Alignment VerticalAlignment,
            int IndentSize,
            double CaretBlinkRate,
            Border CaretStyle)
        {
            this.CellSize = CellSize;
            this.HorizontalAlignment = HorizontalAlignment;
            this.VerticalAlignment = VerticalAlignment;
            this.IndentSize = IndentSize;
            this.CaretBlinkRate = CaretBlinkRate;
            this.CaretStyle = CaretStyle;
        }


        /// <summary>
        /// The size of a cell in the text block.
        /// </summary>
        public readonly Point CellSize;

        /// <summary>
        /// The horizontal alignment of items within their cells.
        /// </summary>
        public readonly Alignment HorizontalAlignment;

        /// <summary>
        /// The vertical alignment of items within their cells.
        /// </summary>
        public readonly Alignment VerticalAlignment;

        /// <summary>
        /// The size of an indent span.
        /// </summary>
        public readonly int IndentSize;

        /// <summary>
        /// The amount of time, in seconds, between blinks for a selection caret.
        /// </summary>
        public readonly double CaretBlinkRate;

        /// <summary>
        /// The style for a selection caret.
        /// </summary>
        public readonly Border CaretStyle;

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
    /// Contains styling information for a section of text in a text block.
    /// </summary>
    public class TextStyle
    {
        public TextStyle(Font Font, Color Back)
        {
            this.Font = Font;
            this.Back = Back;
        }

        /// <summary>
        /// The font for text.
        /// </summary>
        public readonly Font Font;

        /// <summary>
        /// The back color for the text.
        /// </summary>
        public readonly Color Back;
    }

    /// <summary>
    /// A section of text displayable by a text block, excluding styling information.
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
            this.A = A;
            this.B = B;
        }

        /// <summary>
        /// The first of the components of the text section.
        /// </summary>
        public readonly TextSection A;

        /// <summary>
        /// The second of the components of the text section.
        /// </summary>
        public readonly TextSection B;
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

    /// <summary>
    /// A section of text displayable by a text block with styling information included.
    /// </summary>
    public abstract class StyledTextSection
    {
        /// <summary>
        /// Gets a text section that contains the unstyled items of this styled text section.
        /// </summary>
        public TextSection Source
        {
            get
            {
                UniformStyledTextSection usts = this as UniformStyledTextSection;
                if (usts != null)
                {
                    return usts.Source;
                }

                ConcatStyledTextSection csts = this as ConcatStyledTextSection;
                if (csts != null)
                {
                    return csts.A.Source + csts.B.Source;
                }

                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the amount of items in this styled text section.
        /// </summary>
        public int Size
        {
            get
            {
                UniformStyledTextSection usts = this as UniformStyledTextSection;
                if (usts != null)
                {
                    return usts.Source.Size;
                }

                ConcatStyledTextSection csts = this as ConcatStyledTextSection;
                if (csts != null)
                {
                    return csts.Size;
                }

                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the styled text section for the items between the given positions.
        /// </summary>
        public StyledTextSection Subsection(int Start, int End)
        {
            UniformStyledTextSection usts = this as UniformStyledTextSection;
            if (usts != null)
            {
                return new UniformStyledTextSection(usts.Source.Subsection(Start, End), usts.NormalStyle, usts.SelectedStyle);
            }

            ConcatStyledTextSection csts = this as ConcatStyledTextSection;
            if (csts != null)
            {
                int asize = csts.A.Size;
                if (Start >= asize)
                    return csts.B.Subsection(Start - asize, End - asize);
                if (End <= asize)
                    return csts.A.Subsection(Start, End);
                return csts.A.Subsection(Start, asize) + csts.B.Subsection(0, End - asize);

            }

            throw new NotImplementedException();
        }

        public static ConcatStyledTextSection operator +(StyledTextSection A, StyledTextSection B)
        {
            return new ConcatStyledTextSection(A, B);
        }
    }

    /// <summary>
    /// A styled text section with a single uniform style.
    /// </summary>
    public sealed class UniformStyledTextSection : StyledTextSection
    {
        public UniformStyledTextSection(TextSection Source, TextStyle NormalStyle, TextStyle SelectedStyle)
        {
            this.Source = Source;
            this.NormalStyle = NormalStyle;
            this.SelectedStyle = SelectedStyle;
        }

        /// <summary>
        /// The source of the items for this styled text section.
        /// </summary>
        public readonly new TextSection Source;

        /// <summary>
        /// The style for the text section when not selected.
        /// </summary>
        public readonly TextStyle NormalStyle;

        /// <summary>
        /// The style for the text section when selected.
        /// </summary>
        public readonly TextStyle SelectedStyle;
    }

    /// <summary>
    /// A styled text section made by concating two styled text sections.
    /// </summary>
    public sealed class ConcatStyledTextSection : StyledTextSection
    {
        public ConcatStyledTextSection(StyledTextSection A, StyledTextSection B)
        {
            this.Size = A.Size + B.Size;
            this.A = A;
            this.B = B;
        }

        /// <summary>
        /// The total amount of items in this text section.
        /// </summary>
        public readonly new int Size;

        /// <summary>
        /// The first of the components of this styled text section.
        /// </summary>
        public readonly StyledTextSection A;

        /// <summary>
        /// The second of the components of this styled text section.
        /// </summary>
        public readonly StyledTextSection B;
    }
}