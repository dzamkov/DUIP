using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that displays inner controls and items arranged in lines.
    /// </summary>
    public class FlowControl : Control
    {
        public FlowControl()
        {

        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            throw new NotImplementedException();
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                throw new NotImplementedException();
            }

            public override void Render(RenderContext Context)
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Gives information about the arrangement of items within a flow control.
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

        /// <summary>
        /// Items are aligned only to the end of a line.
        /// </summary>
        ReverseRagged,
    }

    /// <summary>
    /// Gives a possible method to determine the size of a flow layout.
    /// </summary>
    public enum FlowFitMode
    {
        /// <summary>
        /// The size of the flow layout is choosen so that the fewest amount of lines are used.
        /// </summary>
        Compact,

        /// <summary>
        /// The size of the flow layout is choosen to be (for general purposes) aesthetically pleasing and readable.
        /// </summary>
        Best
    }

    /// <summary>
    /// Contains functions related to flow blocks and flowing items.
    /// </summary>
    public static class Flow
    {
        /// <summary>
        /// A layout item that can be placed in a flow.
        /// </summary>
        public abstract class Item
        {

        }

        /// <summary>
        /// A item placed in a line that has no additional effects on the arrangement of other items.
        /// </summary>
        public abstract class StandardItem : Item
        {
            /// <summary>
            /// Gets the size of the item.
            /// </summary>
            public abstract Point Size { get; }

            /// <summary>
            /// Gets the position of this item along the line it was placed in.
            /// </summary>
            /// <remarks>This position will start at 0.0 and ascend for successive items, regardless of the flow direction.</remarks>
            public double MinorPosition
            {
                get
                {
                    return this._MinorPosition;
                }
            }

            /// <summary>
            /// Gets the position of this item in relation to the flow container, given the line the item is in, the size of the container,
            /// and the style used for the container.
            /// </summary>
            public Point GetPosition(Line Line, Point ContainerSize, FlowStyle Style)
            {
                ContainerSize = ContainerSize.Shift(Style.MinorAxis);
                Point size = this.Size.Shift(Style.MinorAxis);
                double lineoffset = Align.AxisOffset(Style.LineAlignment, Line.MajorSize, size.Y);
                return new Point(
                    (int)Style.Direction % 4 < 2 ? this._MinorPosition : ContainerSize.X - this._MinorPosition - size.X,
                    ((int)Style.Direction % 2 < 1 ? Line.MajorPosition : ContainerSize.Y - Line.MajorPosition - Line.MajorSize) + lineoffset).Shift(Style.MinorAxis);
            }

            internal double _MinorPosition;
        }
        
        /// <summary>
        /// An ordered collection of items to be placed on the same line.
        /// </summary>
        public class GroupItem : Item
        {
            public GroupItem()
            {

            }

            public GroupItem(List<Item> Items)
            {
                this.Items = Items;
            }

            /// <summary>
            /// The items of this group.
            /// </summary>
            public List<Item> Items;
        }

        /// <summary>
        /// Represents a line of flowed items.
        /// </summary>
        public class Line
        {
            internal Line(FlowStyle Style)
            {
                this._Items = new List<Item>();
                this._MajorSize = Style.LineSize;
                this._UsedLength = 0.0;
            }

            /// <summary>
            /// Gets the size of this line on the major axis.
            /// </summary>
            public double MajorSize
            {
                get
                {
                    return this._MajorSize;
                }
            }

            /// <summary>
            /// Gets the position of this line on the major axis.
            /// </summary>
            /// <remarks>This position will start at 0.0 and ascend for successive lines, regardless of the flow direction.</remarks>
            public double MajorPosition
            {
                get
                {
                    return this._MajorPosition;
                }
            }

            /// <summary>
            /// Gets the total length along the minor axis of the items in this line.
            /// </summary>
            public double UsedLength
            {
                get
                {
                    return this._UsedLength;
                }
            }

            /// <summary>
            /// Gets the items in this line.
            /// </summary>
            public IEnumerable<Item> Items
            {
                get
                {
                    return this._Items;
                }
            }

            /// <summary>
            /// Tries appending the given item to the end of this line. Returns true if it was successfully add or false
            /// if it can not be added.
            /// </summary>
            internal bool _TryAppend(Item Item, double MinorSize, FlowStyle Style)
            {
                StandardItem si = Item as StandardItem;
                if (si != null)
                {
                    Point size = si.Size.Shift(Style.MinorAxis);
                    double nusedlength = this._UsedLength + size.X;
                    if (nusedlength <= MinorSize)
                    {
                        this._UsedLength = nusedlength;
                        this._MajorSize = Math.Max(this._MajorSize, size.Y);
                        this._Items.Add(si);
                        return true;
                    }
                    return false;
                }

                GroupItem gi = Item as GroupItem;
                if (gi != null)
                {
                    double len = 0.0;
                    double hei = this._MajorSize;
                    foreach (Item subitem in gi.Items)
                    {
                        si = subitem as StandardItem;
                        if (si != null)
                        {
                            Point size = si.Size.Shift(Style.MinorAxis);
                            len += size.X;
                            hei = Math.Max(hei, size.Y);
                        }
                    }

                    double nusedlength = this._UsedLength + len;
                    if (nusedlength <= MinorSize)
                    {
                        this._UsedLength = nusedlength;
                        this._MajorSize = hei;
                        this._Items.AddRange(gi.Items);
                        return true;
                    }
                    return false;
                }

                throw new NotImplementedException();
            }

            /// <summary>
            /// Performs layout on the items in this line. The items are assumed to be final, with no more to be added in the future.
            /// </summary>
            internal void _Layout(double MajorPosition, double MinorSize, FlowStyle Style)
            {
                Axis minoraxis = Style.MinorAxis;
                this._MajorPosition = MajorPosition;

                double cur;
                switch (Style.Justification)
                {
                    case FlowJustification.Ragged:
                        cur = 0.0;
                        foreach (Item item in this._Items)
                        {
                            StandardItem si = item as StandardItem;
                            if (si != null)
                            {
                                si._MinorPosition = cur;
                                cur += si.Size[minoraxis];
                                continue;
                            }
                        }
                        break;
                    case FlowJustification.Justify:
                        cur = 0.0;
                        double spacing = this._UsedLength > MinorSize * 0.7 ? (MinorSize - this._UsedLength) / this._Items.Count : 0.0;
                        foreach (Item item in this._Items)
                        {
                            StandardItem si = item as StandardItem;
                            if (si != null)
                            {
                                si._MinorPosition = cur;
                                cur += si.Size[minoraxis];
                                cur += spacing;
                                continue;
                            }
                        }
                        break;
                }
            }


            private List<Item> _Items;
            private double _UsedLength;
            private double _MajorSize;
            private double _MajorPosition;
        }

        /// <summary>
        /// Performs layout on a collection of flow items. Returns a list of the lines the items are arranged in.
        /// </summary>
        /// <param name="MinorSize">The allowable size on the minor axis for layout.</param>
        /// <param name="MajorSize">The size required on the major axis to place all items.</param>
        public static List<Line> Layout(IEnumerable<Item> Items, FlowStyle Style, double MinorSize, out double MajorSize)
        {
            List<Line> lines = _BuildLines(Items, Style, MinorSize);
            _LayoutLines(lines, Style, MinorSize, out MajorSize);
            return lines;
        }

        /// <summary>
        /// Performs layout on a collection of flow items using the given fitting mode. Returns null if no possible arrangement to fit all the items is found.
        /// </summary>
        public static List<Line> Layout(IEnumerable<Item> Items, FlowStyle Style, FlowFitMode Mode, Rectangle SizeRange, out Point Size, out double MajorSize)
        {
            switch (Mode)
            {
                case FlowFitMode.Compact:
                    return LayoutCompact(Items, Style, SizeRange, out Size, out MajorSize);
                default:
                    return LayoutBest(Items, Style, SizeRange, out Size, out MajorSize);
            }
        }

        /// <summary>
        /// Performs layout on a collection of flow items while selecting the smallest possible major (first priority) and
        /// minor sizes. Returns null if no possible arrangement to fit all the items is found.
        /// </summary>
        /// <param name="MajorSize">The smallest possible major size that can keep the layout.</param>
        public static List<Line> LayoutCompact(IEnumerable<Item> Items, FlowStyle Style, Rectangle SizeRange, out Point Size, out double MajorSize)
        {
            List<Line> lines = _BuildLines(Items, Style, SizeRange.Right);
            if (lines == null)
            {
                Size = Point.Zero;
                MajorSize = 0.0;
                return null;
            }
            double minorsize = Math.Max(_GetMinMinorSize(lines), SizeRange.Left);
            _LayoutLines(lines, Style, minorsize, out MajorSize);
            Size = new Point(minorsize, Math.Max(MajorSize, SizeRange.Top));
            return lines;
        }

        /// <summary>
        /// Performs layout on a collection of flow items while selecting an aesthetically pleasing
        /// size within the given size range (with the minor axis being X, and the major axis being Y). 
        /// Returns a list of the lines the items are arranged in. Returns null if no possible 
        /// arrangement to fit all the items is found.
        /// </summary>
        /// <param name="MajorSize">The smallest possible major size that can keep the layout.</param>
        public static List<Line> LayoutBest(IEnumerable<Item> Items, FlowStyle Style, Rectangle SizeRange, out Point Size, out double MajorSize)
        {
            // Try minimizing score by selecting a good minor size.
            double bestscore = double.PositiveInfinity;
            double bestsize = 0.0;
            List<Line> best = null;
            double minorsize = SizeRange.Right;
            while (true)
            {
                List<Line> lines = _BuildLines(Items, Style, minorsize);
                if (lines == null)
                {
                    break;
                }

                // Get the major size required for the layout
                double majorsize = _GetMajorSize(lines, Style);
                if (majorsize > SizeRange.Bottom)
                {
                    break;
                }
                majorsize = Math.Max(majorsize, SizeRange.Top);
                minorsize = Math.Max(_GetMinMinorSize(lines), SizeRange.Left);

                // Compare scores
                double score = _GetScore(minorsize, majorsize, SizeRange, Style);
                if (score < bestscore)
                {
                    bestscore = score;
                    bestsize = minorsize;
                    best = lines;
                }

                // Continue with the next minor size, if possible
                minorsize = minorsize * 0.9;
                if (minorsize < SizeRange.Left)
                {
                    break;
                }
            }

            if (best != null)
            {
                _LayoutLines(best, Style, bestsize, out MajorSize);
                Size = new Point(bestsize, Math.Max(MajorSize, SizeRange.Top));
                return best;
            }
            Size = Point.Zero;
            MajorSize = 0.0;
            return null;
        }

        /// <summary>
        /// Gets the aesthetic score of the given layout. The lower the score, the better the layout.
        /// </summary>
        private static double _GetScore(double MinorSize, double MajorSize, Rectangle SizeRange, FlowStyle Style)
        {
            double aspectratio = MinorSize / MajorSize;
            return (MinorSize - SizeRange.Left) * (MajorSize - SizeRange.Top) * (aspectratio + 1.0 / aspectratio);
        }

        /// <summary>
        /// Tries arranging the given items into lines using the given style and minor size. Returns null if not possible.
        /// </summary>
        private static List<Line> _BuildLines(IEnumerable<Item> Items, FlowStyle Style, double MinorSize)
        {
            List<Line> lines = new List<Line>();
            Line curline = null;
            foreach (Item item in Items)
            {
                if (curline == null || !curline._TryAppend(item, MinorSize, Style))
                {
                    curline = new Line(Style);
                    lines.Add(curline);
                    if (!curline._TryAppend(item, MinorSize, Style))
                    {
                        return null;
                    }
                }
            }
            return lines;
        }

        /// <summary>
        /// Performs layout on lines and items within them.
        /// </summary>
        private static void _LayoutLines(IEnumerable<Line> Lines, FlowStyle Style, double MinorSize, out double MajorSize)
        {
            double majorposition = 0.0;
            double majorsize = 0.0;
            foreach (Line line in Lines)
            {
                line._Layout(majorposition, MinorSize, Style);
                majorsize = majorposition + line.MajorSize;
                majorposition = majorsize + Style.LineSpacing;
            }
            MajorSize = majorsize;
        }

        /// <summary>
        /// Gets the major size needed to display the given lines using the given style.
        /// </summary>
        private static double _GetMajorSize(IEnumerable<Line> Lines, FlowStyle Style)
        {
            double majorposition = 0.0;
            double majorsize = 0.0;
            foreach (Line line in Lines)
            {
                majorsize = majorposition + line.MajorSize;
                majorposition = majorsize + Style.LineSpacing;
            }
            return majorsize;
        }

        /// <summary>
        /// Gets the smallest minor size needed to contain all of the given lines.
        /// </summary>
        private static double _GetMinMinorSize(IEnumerable<Line> Lines)
        {
            double minminorsize = 0.0;
            foreach (Line line in Lines)
            {
                minminorsize = Math.Max(minminorsize, line.UsedLength);
            }
            return minminorsize;
        }
    }
}