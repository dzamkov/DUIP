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
                /// The (foreground) color of the text.
                /// </summary>
                public Color Color;

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
        public void AddText(string String, Font Font, Color Color, bool BreakWords)
        {
            this._Items.Add(new Item.Text
            {
                String = String,
                Font = Font,
                Color = Color,
                BreakWords = BreakWords
            });
        }

        /// <summary>
        /// Appends text to this flowblock.
        /// </summary>
        public void AddText(string String, Font Font, Color Color)
        {
            this.AddText(String, Font, Color, false);
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
            IEnumerable<FlowBlock.Item> sourceitems = Block.Items;
            double minorsize = Environment.SizeRange.TopLeft[Point.GetAxis(style.MinorDirection)];
            double majorsize;

            // Create and layout items
            List<List<_LayoutItem>> linedlayoutitems = new List<List<_LayoutItem>>();
            _GenerateItems(Environment, minorsize, sourceitems, out this._VisualItems, out this._LayoutItems);
            _PerformLayout(this._LayoutItems, style, minorsize, out majorsize, out linedlayoutitems);

            // Get size
            this._Size = new Point(minorsize, majorsize);
            if (Point.GetAxis(style.MinorDirection) == Axis.Vertical)
                this._Size = this._Size.Swap;
        }

        public override Point Size
        {
            get 
            {
                return this._Size;
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            
        }

        public override void Render(RenderContext Context)
        {
            foreach (_LayoutItem li in this._LayoutItems)
            {
                _CharacterLayoutItem cli = li as _CharacterLayoutItem;
                if (cli != null)
                {
                    Figure glyph = cli.Text.Font.GetGlyph(cli.Name, Color.Black);
                    glyph.WithTranslate(cli.TopLeft).Render(Context);
                    ((Disposable<Figure>)glyph).Dispose();
                }
            }
        }

        /// <summary>
        /// A discrete item within a line of a flow control. This type of item contributes to the layout
        /// of the control but can not be interacted with directly.
        /// </summary>
        private class _LayoutItem
        {

        }

        /// <summary>
        /// A layout item for a character in a text.
        /// </summary>
        private class _CharacterLayoutItem : _LayoutItem
        {
            /// <summary>
            /// The top-left corner of the character.
            /// </summary>
            public Point TopLeft;

            /// <summary>
            /// The text this character is associated with.
            /// </summary>
            public _TextVisualItem Text;

            /// <summary>
            /// The offset of this character in the text.
            /// </summary>
            public int Offset;

            /// <summary>
            /// The name of the character.
            /// </summary>
            public char Name;
        }

        /// <summary>
        /// A combination of layout items to be placed on the same line whenever possible. Groups are broken into their parts
        /// during the layout phase.
        /// </summary>
        private class _GroupLayoutItem : _LayoutItem
        {
            /// <summary>
            /// The items in this group.
            /// </summary>
            public List<_LayoutItem> Items;
        }

        /// <summary>
        /// An item that can be rendered, updated, and disposed. Usually linked to one or more layout items.
        /// </summary>
        private class _VisualItem
        {

        }

        /// <summary>
        /// A visual item for text. 
        /// </summary>
        private class _TextVisualItem : _VisualItem
        {
            /// <summary>
            /// The font used for this text.
            /// </summary>
            public Font Font;
        }

        /// <summary>
        /// Gets the minor length of a layout item when added to a line.
        /// </summary>
        private static double _GetMinorLength(_LayoutItem Item, Axis MinorAxis)
        {
            _CharacterLayoutItem cli = Item as _CharacterLayoutItem;
            if (cli != null)
            {
                return cli.Text.Font.GetSize(cli.Name)[MinorAxis];
            }

            _GroupLayoutItem gli = Item as _GroupLayoutItem;
            if (gli != null)
            {
                double tot = 0.0;
                foreach (_LayoutItem li in gli.Items)
                {
                    tot += _GetMinorLength(li, MinorAxis);
                }
                return tot;
            }

            return 0.0;
        }

        /// <summary>
        /// Appends a layout item to the given line.
        /// </summary>
        private static void _AppendLine(_LayoutItem Item, List<_LayoutItem> Line)
        {
            _GroupLayoutItem gli = Item as _GroupLayoutItem;
            if (gli != null)
            {
                foreach (_LayoutItem li in gli.Items)
                {
                    _AppendLine(li, Line);
                }
                return;
            }

            Line.Add(Item);
        }

        /// <summary>
        /// Arranges a collection of layout items into a flow control by updating their positions and creating a list of the contents of lines.
        /// </summary>
        /// <param name="Items">The items to perform layout on.</param>
        /// <param name="MinorSize">The size of the available area for the flow control in the minor direction.</param>
        /// <param name="MajorSize">The size of the flow control in the major direction.</param>
        /// <param name="Lines">The input items arranged in lines.</param>
        private static void _PerformLayout(
            List<_LayoutItem> Items, 
            FlowStyle Style, double MinorSize, out double MajorSize, 
            out List<List<_LayoutItem>> Lines)
        {
            Axis minoraxis = Point.GetAxis(Style.MinorDirection);
            Lines = new List<List<_LayoutItem>>();

            // Build lines
            int next = 0;
            while (next < Items.Count)
            {
                // Determine what items should be on the line
                List<_LayoutItem> line = new List<_LayoutItem>();
                Lines.Add(line);
                double linelength = 0.0;
                while (next < Items.Count)
                {
                    _LayoutItem curitem = Items[next];
                    double minorlength = _GetMinorLength(curitem, minoraxis);
                    if (minorlength + linelength < MinorSize)
                    {
                        linelength += minorlength;
                        _AppendLine(curitem, line);
                        next++;
                    }
                    else
                    {
                        break;
                    }
                }

                
            }

            MajorSize = 1.0;
        }

        /// <summary>
        /// Creates visual and layout items for the given source items. Position information for layout items is not calculated.
        /// </summary>
        private static void _GenerateItems(
            ControlEnvironment Environment, 
            double MinorSize,
            IEnumerable<FlowBlock.Item> SourceItems,
            out List<_VisualItem> VisualItems,
            out List<_LayoutItem> LayoutItems)
        {
            VisualItems = new List<_VisualItem>();
            LayoutItems = new List<_LayoutItem>();

            foreach (FlowBlock.Item item in SourceItems)
            {
                // Text item
                var textitem = item as FlowBlock.Item.Text;
                if (textitem != null)
                {
                    _TextVisualItem tvi = new _TextVisualItem
                    {
                        Font = textitem.Font
                    };
                    VisualItems.Add(tvi);

                    // TODO: obey "BreakWords"
                    int o = 0;
                    foreach (char c in textitem.String)
                    {
                        LayoutItems.Add(new _CharacterLayoutItem
                        {
                            Name = c,
                            Offset = o++,
                            Text = tvi
                        });
                    }
                }


            }
        }

        private List<_LayoutItem> _LayoutItems;
        private List<_VisualItem> _VisualItems;
        private Point _Size;
    }

    /// <summary>
    /// Gives information about the arrangement of items within a flow block.
    /// </summary>
    public struct FlowStyle
    {
        /// <summary>
        /// The justification mode for items.
        /// </summary>
        public FlowJustification Justification;

        /// <summary>
        /// Gets the direction lines are arranged in the flow block.
        /// </summary>
        public Direction MajorDirection;

        /// <summary>
        /// Gets the direction items within lines are arranged in the flow block.
        /// </summary>
        public Direction MinorDirection;

        /// <summary>
        /// The minimum size, in the major direction, of a line.
        /// </summary>
        public double MinLineSize;

        /// <summary>
        /// The maximum size, in the major direction, of a line.
        /// </summary>
        public double MaxLineSize;

        /// <summary>
        /// The amount of space, in the major direction, between lines.
        /// </summary>
        public double LineSpacing;
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
}