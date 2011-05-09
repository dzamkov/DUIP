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

        public FlowBlock(IEnumerable<Item> Items)
        {
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
                /// Should words be broken between lines?
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
            throw new NotImplementedException();
        }

        private List<Item> _Items;
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