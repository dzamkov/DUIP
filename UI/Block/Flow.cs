using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that displays inner blocks and items arranged in lines.
    /// </summary>
    public class FlowBlock : Block
    {
        public FlowBlock()
        {

        }

        public FlowBlock(FlowStyle Style, FlowFit Fit, List<FlowItem> Items)
        {
            this._Style = Style;
            this._Fit = Fit;
            this._Items = Items;
        }

        /// <summary>
        /// Gets or sets the style of the flow for the block.
        /// </summary>
        [StaticProperty]
        public FlowStyle Style
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
        /// Gets or sets the method used to determine the size of the block.
        /// </summary>
        [StaticProperty]
        public FlowFit Fit
        {
            get
            {
                return this._Fit;
            }
            set
            {
                this._Fit = value;
            }
        }

        /// <summary>
        /// Gets or sets the items in the flow block.
        /// </summary>
        [StaticProperty]
        public List<FlowItem> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
            }
        }

        public override Layout CreateLayout(InputContext Context, Rectangle SizeRange, out Point Size)
        {
            FlowStyle style = this.Style;
            Axis minoraxis = style.MinorAxis;
            SizeRange.TopLeft = SizeRange.TopLeft.Shift(minoraxis);
            SizeRange.BottomRight = SizeRange.BottomRight.Shift(minoraxis);

            // Determine minor size
            double minor = 0.0;
            if (this._Fit == CompactFlowFit.Instance)
            {
                minor = SizeRange.Right;
            }
            else
            {
                if (SizeRange.Left == SizeRange.Right)
                {
                    minor = SizeRange.Left;
                }
                else
                {
                    double pmajor;
                    minor = this._PickMinor(SizeRange.Left, SizeRange.Right, out pmajor);
                }
            }

            // Create lines
            List<_PlannedLine> lines;
            switch (style.WrapMode)
            {
                case FlowWrap.Greedy:
                    lines = _GetLinesGreedy(this.Items, minor, SizeRange.Right, style);
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Create a space layout if lines can not be created
            if (lines == null)
            {
                Size = SizeRange.TopLeft;
                return SpaceBlock.Layout;
            }

            // Get minimum minor size needed to display all lines (this becomes the new minor size).
            double lminor = minor;
            minor = SizeRange.Left;
            foreach (_PlannedLine pl in lines)
            {
                double waste = lminor - pl.Length;
                minor = Math.Max(minor, pl.Length);
            }
            
            // Build layout lines
            double major;
            List<_Layout.Line> layoutlines = _BuildLayout(lines, this._Items, style, minor, SizeRange.Top, out major);

            // Create a space layout if the major size exceeds the size range
            if (major > SizeRange.Bottom + Layout.ErrorThreshold)
            {
                Size = SizeRange.TopLeft;
                return SpaceBlock.Layout;
            }

            // Create layout
            Size = new Point(minor, major).Shift(style.MinorAxis);
            return new _Layout
            {
                Block = this,
                Lines = layoutlines
            };
        }

        private class _Layout : Layout
        {
            public override Figure Figure
            {
                get
                {
                    FlowStyle style = this.Block.Style;
                    Axis minoraxis = style.MinorAxis;
                    Alignment linealign = style.LineAlignment;

                    Figure fig = null;
                    foreach (Line line in this.Lines)
                    {
                        double majoff = line.MajorOffset;
                        double majsize = line.MajorSize;
                        foreach (Item item in line.Items)
                        {
                            FlowItem source = item.Source;
                            double minoff = item.MinorOffset;

                            // Character item
                            CharacterFlowItem cfi = source as CharacterFlowItem;
                            if (cfi != null)
                            {
                                Font font = cfi.Font;
                                char name = cfi.Name;

                                Point size = font.GetSize(name).Shift(minoraxis);
                                Point off = new Point(minoff, majoff + Align.AxisOffset(linealign, majsize, size.Y)).Shift(minoraxis);
                                fig ^= font.GetGlyph(name).Translate(off);
                            }
                        }
                    }
                    return fig;
                }
            }

            /// <summary>
            /// A line within a constructed layout.
            /// </summary>
            public class Line
            {
                /// <summary>
                /// The offset of the line along the major axis.
                /// </summary>
                public double MajorOffset;

                /// <summary>
                /// The size of the line along the major axis.
                /// </summary>
                public double MajorSize;

                /// <summary>
                /// The items in this line.
                /// </summary>
                public Item[] Items;
            }

            /// <summary>
            /// An item within a constructed layout.
            /// </summary>
            public struct Item
            {
                /// <summary>
                /// The offset of the item along the minor axis.
                /// </summary>
                public double MinorOffset;

                /// <summary>
                /// The size of the item along the minor axis.
                /// </summary>
                public double MinorSize;

                /// <summary>
                /// The flow item this item is derived from.
                /// </summary>
                public FlowItem Source;
            }

            public FlowBlock Block;
            public List<Line> Lines;
        }

        /// <summary>
        /// A description of a valid line starting at a certain item that may be added to a flow.
        /// </summary>
        private struct _PlannedLine
        {
            /// <summary>
            /// The amount of items in this line.
            /// </summary>
            public int Size;

            /// <summary>
            /// The index of the first item in the following line.
            /// </summary>
            public int Next;

            /// <summary>
            /// The minimum length of this line along the minor axis.
            /// </summary>
            public double Length;

            /// <summary>
            /// Indicates wether this line is ended with a cut item.
            /// </summary>
            public bool Cut;
        }

        /// <summary>
        /// Gets the possible lines that may be formed from the given items.
        /// </summary>
        /// <param name="Start">The item to start the lines on.</param>
        /// <param name="Prefered">The prefered size of a line.</param>
        /// <param name="Max">The maximum size of a line.</param>
        private static IEnumerable<_PlannedLine> _GetPossibleLines(List<FlowItem> Items, int Start, double Prefered, double Max, FlowStyle Style)
        {
            int cur = Start;
            int size = 0;
            double len = 0.0;
            bool hasline = false;
            while (cur < Items.Count)
            {
                // Insure line does not exceed size limit
                if (len - Layout.ErrorThreshold > Prefered && hasline)
                {
                    yield break;
                }
                if (len - Layout.ErrorThreshold > Max)
                {
                    yield break;
                }

                // Examine current item
                FlowItem item = Items[cur];

                // Break item
                if (item == BreakFlowItem.Instance)
                {
                    yield return new _PlannedLine
                    {
                        Length = len,
                        Next = cur + 1,
                        Size = size,
                        Cut = false
                    };
                    hasline = true;
                    cur++;
                    size++;
                    continue;
                }

                // Cut item
                if (item == CutFlowItem.Instance)
                {
                    yield return new _PlannedLine
                    {
                        Length = len,
                        Next = cur + 1,
                        Size = size,
                        Cut = true
                    };
                    yield break;
                }

                // Character item
                CharacterFlowItem cfi = item as CharacterFlowItem;
                if (cfi != null)
                {
                    len += cfi.Font.GetSize(cfi.Name)[Style.MinorAxis];
                    cur++;
                    size++;
                    continue;
                }

                // Space item
                SpaceFlowItem sfi = item as SpaceFlowItem;
                if (sfi != null)
                {
                    if (sfi.Breaking)
                    {
                        yield return new _PlannedLine
                        {
                            Length = len,
                            Next = cur + 1,
                            Size = size,
                            Cut = false
                        };
                        hasline = true;
                    }
                    len += sfi.Length;
                    cur++;
                    size++;
                    continue;
                }
            }

            // End of items can act as a valid break
            yield return new _PlannedLine
            {
                Length = len,
                Next = Items.Count,
                Size = size,
                Cut = false
            };
        }

        /// <summary>
        /// Greedily creates a list of lines for a sequence of items, or returns null if not possible.
        /// </summary>
        /// <param name="Prefered">The prefered size of a line.</param>
        /// <param name="Max">The maximum size of a line.</param>
        private static List<_PlannedLine> _GetLinesGreedy(List<FlowItem> Items, double Prefered, double Max, FlowStyle Style)
        {
            int cur = 0;
            List<_PlannedLine> lines = new List<_PlannedLine>();
            while (cur < Items.Count)
            {
                bool hasline = false;
                _PlannedLine last = new _PlannedLine();
                foreach(_PlannedLine line in _GetPossibleLines(Items, cur, Prefered, Max, Style))
                {
                    hasline = true;
                    last = line;
                }

                // Exit if no lines are found.
                if (!hasline)
                {
                    return null;
                }

                cur = last.Next;
                lines.Add(last);
            }
            return lines;
        }

        /// <summary>
        /// Builds the layout lines for a list of planned lines.
        /// </summary>
        private static List<_Layout.Line> _BuildLayout(
            List<_PlannedLine> Lines, List<FlowItem> Items, FlowStyle Style, 
            double MinorSize, double MinMajorSize, out double MajorSize)
        {
            List<_Layout.Line> lines = new List<_Layout.Line>(Lines.Count);
            int cur = 0;

            double majoroff = 0.0;
            MajorSize = 0.0;

            // Build lines
            foreach (_PlannedLine pl in Lines)
            {
                _Layout.Line line = _BuildLayoutLine(cur, pl.Size, pl.Length, pl.Cut, Items, Style, MinorSize);
                lines.Add(line);
                cur = pl.Next;

                line.MajorOffset = majoroff;
                MajorSize = majoroff + line.MajorSize;
                majoroff = MajorSize + Style.LineSpacing;
            }
            MajorSize = Math.Max(MajorSize, MinMajorSize);

            // Reverse line direction if needed
            if ((int)Style.Direction % 2 > 0)
            {
                majoroff = MajorSize;
                foreach (_Layout.Line line in lines)
                {
                    line.MajorOffset = majoroff - line.MajorSize;
                    majoroff = line.MajorOffset - Style.LineSpacing;
                }
            }

            return lines;
        }

        /// <summary>
        /// Builds a layout line for a given subset of a list of items. The major offset of the line will not be set.
        /// </summary>
        /// <param name="Length">The known minimum length of the line.</param>
        private static _Layout.Line _BuildLayoutLine(int Start, int Size, double Length, bool Cut, List<FlowItem> Items, FlowStyle Style, double MinorSize)
        {
            Axis minoraxis = Style.MinorAxis;
            double majorsize = Style.LineSize;
            _Layout.Item[] items = new _Layout.Item[Size];

            // Add visible items while determining their sizes
            double totalspace = 0.0;
            for (int t = 0; t < items.Length; t++)
            {
                FlowItem item = Items[Start++];

                // Character item
                CharacterFlowItem cfi = item as CharacterFlowItem;
                if (cfi != null)
                {
                    Point size = cfi.Font.GetSize(cfi.Name).Shift(Style.MinorAxis);
                    majorsize = Math.Max(majorsize, size.Y);
                    items[t] = new _Layout.Item
                    {
                        MinorSize = size.X,
                        Source = cfi
                    };
                }

                // Space item
                SpaceFlowItem sfi = item as SpaceFlowItem;
                if (sfi != null)
                {
                    totalspace += sfi.Length;
                    items[t] = new _Layout.Item
                    {
                        MinorSize = sfi.Length,
                        Source = sfi
                    };
                }
            }

            // If the flow is justified, adjust the sizes of spaces
            if (Style.Justification == FlowJustification.Justify && !Cut)
            {
                double spacemult = (MinorSize - Length + totalspace) / totalspace;
                for (int t = 0; t < items.Length; t++)
                {
                    if (items[t].Source is SpaceFlowItem)
                    {
                        items[t].MinorSize *= spacemult;
                    }
                }
            }

            // Set item offsets
            if ((int)Style.Direction % 4 < 2)
            {
                double off = Style.Justification == FlowJustification.Center ? MinorSize * 0.5 - Length * 0.5 : 0.0;
                for (int t = 0; t < items.Length; t++)
                {
                    items[t].MinorOffset = off;
                    off += items[t].MinorSize;
                }
            }
            else
            {
                double off = Style.Justification == FlowJustification.Center ? MinorSize * 0.5 + Length * 0.5 : MinorSize;
                for (int t = 0; t < items.Length; t++)
                {
                    off -= items[t].MinorSize;
                    items[t].MinorOffset = off;
                }
            }

            return new _Layout.Line
            {
                MajorSize = majorsize,
                Items = items
            };
        }

        /// <summary>
        /// Contains item measurements used to estimate the sizes and parameters of layouts.
        /// </summary>
        private class _Metrics
        {
            /// <summary>
            /// The average size (in minor and major coordinates) of an item.
            /// </summary>
            public Point AverageSize;

            /// <summary>
            /// The maximum size (in minor and major coordinates) of an item.
            /// </summary>
            public Point MaximumSize;

            /// <summary>
            /// The average amount of space between breaks.
            /// </summary>
            public double AverageBreakSpacing;
        }

        /// <summary>
        /// Creates metrics for this flow.
        /// </summary>
        private _Metrics _CreateMetrics()
        {
            FlowStyle style = this._Style;
            Axis minoraxis = style.MinorAxis;
            double linesize = style.LineSize;

            Point avg = Point.Zero;
            Point max = Point.Zero;
            int breaks = 0;
            foreach (FlowItem item in this._Items)
            {
                CharacterFlowItem cfi = item as CharacterFlowItem;
                if (cfi != null)
                {
                    Point size = cfi.Font.GetSize(cfi.Name).Shift(minoraxis);
                    size.Y = Math.Max(size.Y, linesize);

                    avg += size;
                    max.X = Math.Max(max.X, size.X);
                    max.Y = Math.Max(max.Y, size.Y);
                    continue;
                }

                SpaceFlowItem sfi = item as SpaceFlowItem;
                if (sfi != null) 
                {
                    double len = sfi.Length;
                    avg.X += len;
                    max.X = Math.Max(max.X, len);
                    if (sfi.Breaking)
                    {
                        breaks++;
                    }
                    continue;
                }

                if (item == BreakFlowItem.Instance)
                {
                    breaks++;
                }

                if (item == CutFlowItem.Instance)
                {
                    breaks++;
                }
            }

            double icount = 1.0 / this._Items.Count;
            return new _Metrics
            {
                AverageSize = avg * icount,
                AverageBreakSpacing = avg.X / breaks,
                MaximumSize = max,
            };
        }

        /// <summary>
        /// Gets metrics for this flow.
        /// </summary>
        private _Metrics _GetMetrics()
        {
            if (this._MetricsCache == null)
            {
                return this._MetricsCache = this._CreateMetrics();
            }
            return this._MetricsCache;
        }

        /// <summary>
        /// Picks a minor size in order to satisfy the fit method used.
        /// </summary>
        /// <param name="Major">The predicted major size of the layout.</param>
        private double _PickMinor(double Min, double Max, out double Major)
        {
            FlowStyle style = this._Style;
            FlowFit fit = this._Fit;
            _Metrics metrics = this._GetMetrics();
            int itemcount = this._Items.Count;

            Point avg = metrics.AverageSize;
            Point max = metrics.MaximumSize;
            double breakspacing = metrics.AverageBreakSpacing;
            double linespacing = style.LineSpacing;

            double totalminor = avg.X * itemcount;
            double majorvary = max.Y - avg.Y;

            // Begin guessing good values for the minor size
            double minminor = Math.Max(Math.Max(avg.X, Min), breakspacing);
            double maxminor = Math.Min(totalminor, Max);
            double deltaminor = (maxminor - minminor) / 100.0;
            double minor = minminor;
            Major = 0.0;
            double score = double.PositiveInfinity;
            for(int t = 0; t < 100; t++)
            {
                double tminor = minminor + t * deltaminor;
                double r = (tminor / totalminor);
                double tlinesize = r * majorvary + avg.Y;
                double twaste = breakspacing;
                double tlines = totalminor / (tminor - twaste);
                double tmajor = tlines * (tlinesize + linespacing) - linespacing;
                double tscore = fit.GetScore(tminor, tmajor, totalminor, tlines);

                if (tscore < score)
                {
                    score = tscore;
                    minor = tminor;
                    Major = tmajor;
                }
            }
            return minor;
        }

        private _Metrics _MetricsCache;
        private FlowFit _Fit;
        private FlowStyle _Style;
        private List<FlowItem> _Items;
    }

    /// <summary>
    /// An item that can appear in a flow. Items can be displayable, or can have
    /// effects on the layout of a flow.
    /// </summary>
    public class FlowItem
    {
        /// <summary>
        /// Gets a flow item that signals a possible break between lines.
        /// </summary>
        public static BreakFlowItem Break
        {
            get
            {
                return BreakFlowItem.Instance;
            }
        }

        /// <summary>
        /// Gets a flow item that ends the line it occurs on.
        /// </summary>
        public static CutFlowItem Cut
        {
            get
            {
                return CutFlowItem.Instance;
            }
        }

        /// <summary>
        /// Creates a flow item that signals a space of a certain length.
        /// </summary>
        public static SpaceFlowItem Space(double Length, bool Breaking)
        {
            return new SpaceFlowItem
            {
                Length = Length,
                Breaking = Breaking
            };
        }

        /// <summary>
        /// Creates a flow item that displays a glyph of a certain font.
        /// </summary>
        public static CharacterFlowItem Character(char Name, Font Font)
        {
            return new CharacterFlowItem
            {
                Name = Name,
                Font = Font
            };
        }

        /// <summary>
        /// Creates a sequence of flow items to display the given text.
        /// </summary>
        public static List<FlowItem> CreateText(string Text, Font Font, double SpaceLength, bool CutEnd)
        {
            List<FlowItem> items = new List<FlowItem>();
            SpaceFlowItem space = Space(SpaceLength, true);
            for (int t = 0; t < Text.Length; t++)
            {
                char c = Text[t];
                switch (c)
                {
                    case ' ':
                        items.Add(space);
                        break;
                    case '\n':
                        items.Add(Cut);
                        break;
                    default:
                        items.Add(Character(c, Font));
                        break;
                }
            }
            if (CutEnd)
            {
                items.Add(FlowItem.Cut);
            }
            return items;
        }
    }

    /// <summary>
    /// An invisible flow item that allows the flow to be broken into seperate lines where it occurs.
    /// </summary>
    public class BreakFlowItem : FlowItem
    {
        private BreakFlowItem()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly BreakFlowItem Instance = new BreakFlowItem();
    }

    /// <summary>
    /// A flow item that forces the current line to end. If the flow is justified, the line this item is on
    /// will be ragged.
    /// </summary>
    public class CutFlowItem : FlowItem
    {
        private CutFlowItem()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly CutFlowItem Instance = new CutFlowItem();
    }

    /// <summary>
    /// A flow item that creates some spacing along the minor axis of a line.
    /// </summary>
    public class SpaceFlowItem : FlowItem
    {
        /// <summary>
        /// Gets the length of the space. The space may be stretched in a justified flow.
        /// </summary>
        public double Length;

        /// <summary>
        /// Determines wether the space can act as a break between lines. If a space does cause a line break, the space will
        /// not be visible.
        /// </summary>
        public bool Breaking;
    }

    /// <summary>
    /// A flow item that displays a glyph of a certain font.
    /// </summary>
    public class CharacterFlowItem : FlowItem
    {
        /// <summary>
        /// The name of the glyph to display.
        /// </summary>
        public char Name;

        /// <summary>
        /// The font to use to display the glyph.
        /// </summary>
        public Font Font;
    }

    /// <summary>
    /// A method of determining the prefered size of a flow block.
    /// </summary>
    public abstract class FlowFit
    {
        /// <summary>
        /// Gets a score of the layout of the given parameters. A flow block will try to minimize this value.
        /// </summary>
        /// <param name="Minor">The size of the layout in the minor direction.</param>
        /// <param name="Major">The size of the layout in the major direction.</param>
        /// <param name="TotalMinor">The total size of all items in the flow, in the minor direction.</param>
        /// <param name="Lines">The amount of lines in the layout.</param>
        public abstract double GetScore(double Minor, double Major, double TotalMinor, double Lines);

        /// <summary>
        /// Gets the compact fit method.
        /// </summary>
        public static CompactFlowFit Compact
        {
            get
            {
                return CompactFlowFit.Instance;
            }
        }

        /// <summary>
        /// Gets an aspect ratio fit method.
        /// </summary>
        public static AspectRatioFlowFit AspectRatio(double Target)
        {
            return new AspectRatioFlowFit(Target);
        }
    }

    /// <summary>
    /// A flow fit method that uses the smallest major size possible.
    /// </summary>
    public sealed class CompactFlowFit : FlowFit
    {
        private CompactFlowFit()
        {

        }

        /// <summary>
        /// The only instance of the class.
        /// </summary>
        public static readonly CompactFlowFit Instance = new CompactFlowFit();

        public override double GetScore(double Minor, double Major, double TotalMinor, double Lines)
        {
            return Major;
        }
    }

    /// <summary>
    /// A flow fit method that finds a size as close to a certain aspect ratio (minor / major size) as possible.
    /// </summary>
    public class AspectRatioFlowFit : FlowFit
    {
        public AspectRatioFlowFit(double AspectRatio)
        {
            this._AspectRatio = AspectRatio;
        }

        /// <summary>
        /// Gets the target aspect ratio for this fit method.
        /// </summary>
        public new double AspectRatio
        {
            get
            {
                return this._AspectRatio;
            }
        }

        public override double GetScore(double Minor, double Major, double TotalMinor, double Lines)
        {
            double c = Minor / Major;
            double t = this._AspectRatio;
            return (c - t) * (1.0 / t - 1.0 / c);
        }

        private double _AspectRatio;
    }

    /// <summary>
    /// Gives information about the arrangement of items within a flow block.
    /// </summary>
    /// <remarks>The minor direction is the direction items within a line follow. The major direction
    /// is the direction lines follow.</remarks>
    public class FlowStyle
    {
        /// <summary>
        /// The justification mode for items.
        /// </summary>
        public FlowJustification Justification;

        /// <summary>
        /// The direction of the flow.
        /// </summary>
        public FlowDirection Direction;

        /// <summary>
        /// The wrap mode for the flow.
        /// </summary>
        public FlowWrap WrapMode;

        /// <summary>
        /// The alignment of items within lines on the major axis.
        /// </summary>
        public Alignment LineAlignment;

        /// <summary>
        /// The minimum size, on the major axis, of a line.
        /// </summary>
        public double LineSize;

        /// <summary>
        /// The amount of space, in the major direction, between lines.
        /// </summary>
        public double LineSpacing;

        /// <summary>
        /// Gets the minor axis for this flow style.
        /// </summary>
        public Axis MinorAxis
        {
            get
            {
                return (int)this.Direction < 4 ? Axis.Horizontal : Axis.Vertical;
            }
        }

        /// <summary>
        /// Gets the major axis for this flow style.
        /// </summary>
        public Axis MajorAxis
        {
            get
            {
                return (int)this.Direction < 4 ? Axis.Vertical : Axis.Horizontal;
            }
        }
    }

    /// <summary>
    /// The direction that lines (major) and items within lines (minor) progress in a flow block. Items within
    /// this enumeration are named with the minor direction followed by the major direction.
    /// </summary>
    public enum FlowDirection
    {
        RightDown,
        RightUp,
        LeftDown,
        LeftUp,
        DownRight,
        DownLeft,
        UpRight,
        UpLeft,
    }

    /// <summary>
    /// Gives a justification mode for flow items.
    /// </summary>
    public enum FlowJustification
    {
        /// <summary>
        /// Items in a line are centered and not aligned with either side of the line.
        /// </summary>
        Center,

        /// <summary>
        /// Items are aligned to both sides of a line.
        /// </summary>
        Justify,

        /// <summary>
        /// Items are aligned only to the beginning of a line.
        /// </summary>
        Ragged,
    }

    /// <summary>
    /// Gives a possible method of choosing where to put automatic line breaks in a flow.
    /// </summary>
    public enum FlowWrap
    {
        /// <summary>
        /// Each line is given as many items as it can fit.
        /// </summary>
        Greedy,

        /// <summary>
        /// Line breaks are choosen to give the best aesthetic results.
        /// </summary>
        Best
    }
}