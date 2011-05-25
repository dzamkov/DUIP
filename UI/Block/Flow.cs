using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that orders other blocks, items and commands sequentially in its interior.
    /// </summary>
    public class FlowBlock : Block
    {
        public FlowBlock()
        {
            this._Items = new List<Item>();
        }

        public FlowBlock(FlowStyle Style, IEnumerable<Item> Items)
        {
            this._Style = Style;
            this._Items = new List<Item>(Items);
        }

        /// <summary>
        /// Gets the items within this flowblock.
        /// </summary>
        public IEnumerable<Item> Items
        {
            get
            {
                return this._Items;
            }
        }

        /// <summary>
        /// Gets or sets the layout style of the flow block.
        /// </summary>
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
        /// An item within a flow block.
        /// </summary>
        public class Item
        {
            /// <summary>
            /// A visual representation of a string laid along a flow block.
            /// </summary>
            public class Text : Item
            {
                /// <summary>
                /// The string representation of the text.
                /// </summary>
                public string String;

                /// <summary>
                /// The font used to display the text.
                /// </summary>
                public Font Font;

                /// <summary>
                /// Are characters within words grouped in the same line?
                /// </summary>
                public bool GroupWords;
            }
        }

        /// <summary>
        /// Appends an item to the end of this flowblock.
        /// </summary>
        public void AddItem(Item Item)
        {
            this._Items.Add(Item);
        }

        /// <summary>
        /// Appends text to this flowblock.
        /// </summary>
        public void AddText(string String, Font Font, bool GroupWords)
        {
            this._Items.Add(new Item.Text
            {
                String = String,
                Font = Font,
                GroupWords = GroupWords
            });
        }

        /// <summary>
        /// Appends text to this flowblock.
        /// </summary>
        public void AddText(string String, Font Font)
        {
            this.AddText(String, Font, true);
        }

        public override Disposable<Control> CreateControl(Rectangle SizeRange, Theme Theme)
        {
            return new FlowControl(this, SizeRange, Theme);
        }

        private FlowStyle _Style;
        private List<Item> _Items;
    }

    /// <summary>
    /// A control for a flow block.
    /// </summary>
    public class FlowControl : Control
    {
        public FlowControl(FlowBlock Block, Rectangle SizeRange, Theme Theme)
        {
            this._FlowStyle = Block.Style;
            double minorsize, majorsize;
            Point minsize = SizeRange.TopLeft.Shift(this._FlowStyle.MinorAxis);
            Point maxsize = SizeRange.BottomRight.Shift(this._FlowStyle.MinorAxis);
            minorsize = minsize.X;

            List<Flow.Item> items = new List<Flow.Item>();
            foreach (FlowBlock.Item it in Block.Items)
            {
                _Append(it, items, this._FlowStyle, SizeRange, Theme);
            }
            this._Lines = Flow.Layout(items, this._FlowStyle, minorsize, out majorsize);
            majorsize = Math.Max(minsize.Y, Math.Min(maxsize.Y, majorsize));

            this._Size = new Point(minorsize, majorsize).Shift(this._FlowStyle.MinorAxis);
        }

        /// <summary>
        /// Appends a flowblock item to a list of layout items.
        /// </summary>
        private static void _Append(FlowBlock.Item Item, List<Flow.Item> Items, FlowStyle Style, Rectangle SizeRange, Theme Theme)
        {
            // Text
            var text = Item as FlowBlock.Item.Text;
            if (text != null)
            {
                if (text.GroupWords)
                {
                    List<Flow.Item> curgroup = new List<Flow.Item>();
                    foreach (char c in text.String)
                    {
                        if (c == ' ')
                        {
                            if (curgroup.Count > 0)
                            {
                                Items.Add(new Flow.GroupItem(curgroup));
                                curgroup = new List<Flow.Item>();
                            }
                            Items.Add(new _CharItem(text.Font, ' '));
                        }
                        else
                        {
                            curgroup.Add(new _CharItem(text.Font, c));
                        }
                    }
                }
                else
                {
                    foreach (char c in text.String)
                    {
                        Items.Add(new _CharItem(text.Font, c));
                    }
                }
            }
        }

        /// <summary>
        /// A layout item for a character of a certain font.
        /// </summary>
        private class _CharItem : Flow.StandardItem
        {
            public _CharItem(Font Font, char Name)
            {
                this._Font = Font;
                this._Name = Name;
                this._Size = this._Font.GetSize(Name);
            }

            public void Render(RenderContext Context, Point Position)
            {
                Disposable<Figure> glyph = this._Font.GetGlyph(this._Name);
                using (Context.Translate(Position))
                {
                    ((Figure)glyph).Render(Context);
                }
                glyph.Dispose();
            }

            public override Point Size
            {
                get
                {
                    return this._Size;
                }
            }

            private Point _Size;
            private Font _Font;
            private char _Name;
        }

        public override Point Size
        {
            get
            {
                return this._Size;
            }
        }

        public override void Render(RenderContext Context)
        {
            foreach (Flow.Line line in this._Lines)
            {
                foreach (Flow.Item item in line.Items)
                {
                    _CharItem ci = item as _CharItem;
                    if (ci != null)
                    {
                        ci.Render(Context, ci.GetPosition(line, this._Size, this._FlowStyle));
                    }
                }
            }
        }

        private List<Flow.Line> _Lines;
        private FlowStyle _FlowStyle;
        private Point _Size;
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
            // Build lines
            List<Line> lines = new List<Line>();
            Line curline = null;
            foreach (Item item in Items)
            {
                if (curline == null || !curline._TryAppend(item, MinorSize, Style))
                {
                    curline = new Line(Style);
                    lines.Add(curline);
                    curline._TryAppend(item, MinorSize, Style);
                }
            }

            // Layout lines
            double majorposition = 0.0;
            double majorsize = 0.0;
            foreach (Line line in lines)
            {
                line._Layout(majorposition, MinorSize, Style);
                majorsize = majorposition + line.MajorSize;
                majorposition = majorsize + Style.LineSpacing;
            }
            MajorSize = majorsize;


            // Return lines
            return lines;
        }
    }
}