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
                /// Can words be broken between lines?
                /// </summary>
                public bool BreakWords;
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
        public void AddText(string String, Font Font, bool BreakWords)
        {
            this._Items.Add(new Item.Text
            {
                String = String,
                Font = Font,
                BreakWords = BreakWords
            });
        }

        /// <summary>
        /// Appends text to this flowblock.
        /// </summary>
        public void AddText(string String, Font Font)
        {
            this.AddText(String, Font, false);
        }

        public override Disposable<Control> CreateControl(ControlEnvironment Environment)
        {
            return new FlowControl(this, Environment);
        }

        private FlowStyle _Style;
        private List<Item> _Items;
    }

    /// <summary>
    /// A control for a flow block.
    /// </summary>
    public class FlowControl : Control
    {
        public FlowControl(FlowBlock Block, ControlEnvironment Environment)
        {
            FlowStyle style = Block.Style;
            double minorsize, majorsize;
            minorsize = Environment.SizeRange.TopLeft[style.MinorAxis];
            this._Items = new List<Flow.Item>();

            foreach (FlowBlock.Item it in Block.Items)
            {
                var ti = it as FlowBlock.Item.Text;
                if (ti != null)
                {
                    foreach (char c in ti.String)
                    {
                        this._Items.Add(new _CharItem(ti.Font, c));
                    }
                }
            }

            Flow.Layout(this._Items, style, minorsize, out majorsize);
            this._Size = new Point(minorsize, majorsize).Shift(style.MinorAxis);
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

            public void Render(RenderContext Context)
            {
                Disposable<Figure> glyph = this._Font.GetGlyph(this._Name);
                using (Context.Translate(this._Position))
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

            public override Point Position
            {
                get
                {
                    return this._Position;
                }
                set
                {
                    this._Position = value;
                }
            }

            private Point _Size;
            private Point _Position;
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
            foreach (Flow.Item item in this._Items)
            {
                _CharItem ci = item as _CharItem;
                if (ci != null)
                {
                    ci.Render(Context);
                }
            }
        }

        private List<Flow.Item> _Items;
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
        public double MinLineSize;

        /// <summary>
        /// The maximum size, on the major axis, of a line.
        /// </summary>
        public double MaxLineSize;

        /// <summary>
        /// The amount of space, in the major direction, between lines.
        /// </summary>
        public double LineSpacing;

        /// <summary>
        /// The amount of padding in the minor direction.
        /// </summary>
        public double MinorPadding;

        /// <summary>
        /// The amount of padding in the major direction.
        /// </summary>
        public double MajorPadding;

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
            /// Gets or sets the position of the topleft corner of the item in relation to the topleft corner of
            /// the flow block.
            /// </summary>
            public abstract Point Position { get; set; }
        }

        /// <summary>
        /// Performs layout on a collection of flow items.
        /// </summary>
        /// <param name="MinorSize">The allowable size on the minor axis for layout.</param>
        /// <param name="MajorSize">The size required on the major axis to place all items.</param>
        public static void Layout(IEnumerable<Item> Items, FlowStyle Style, double MinorSize, out double MajorSize)
        {
            double curmajor = Style.MajorPadding;

            foreach (Item item in Items)
            {

            }

            MajorSize = curmajor + Style.MajorPadding;
        }
    }
}