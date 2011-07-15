using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// Defines user and context dependant information used to display items and content.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// The border to use for nodes where no more specialized border is suitable.
        /// </summary>
        public Border NodeBorder = new Border(0.04, Color.RGB(0.2, 0.2, 0.2));

        /// <summary>
        /// The background color to use for nodes where no more specialized background is suitable.
        /// </summary>
        public Color NodeBackground = Color.RGB(0.95, 0.95, 0.95);

        /// <summary>
        /// Gets the general-purpose flowstyle to use where no more specialized flow style is suitable.
        /// </summary>
        public FlowStyle FlowStyle = new FlowStyle
        {
            Direction = FlowDirection.RightDown,
            Justification = FlowJustification.Justify,
            LineAlignment = Alignment.Center,
            LineSpacing = 0.00,
            LineSize = 0.01
        };

        /// <summary>
        /// A general purpose font.
        /// </summary>
        public Font GeneralFont = new SystemFont("Arial", 0.05, Color.Black);

        /// <summary>
        /// Creates a block that displays general-purpose text.
        /// </summary>
        public Block TextBlock(string Text)
        {
            return new FlowBlock
            {
                Fit = FlowFit.AspectRatio(2.0),
                Style = this.FlowStyle,
                Items = FlowItem.CreateText(Text, this.GeneralFont, 0.01, true)
            }.WithMargin(0.04);
        }

        /// <summary>
        /// Creates a block that displays a number.
        /// </summary>
        public Block NumberBlock(int Number)
        {
            return this.TextBlock(Number.ToString());
        }

        /// <summary>
        /// Gets a block that displays the size of data in bytes.
        /// </summary>
        public Block DataSizeBlock(long Size)
        {
            return this.TextBlock(Size.ToString("n0"));
        }

        /// <summary>
        /// Creates a block that displays a collection of named properties.
        /// </summary>
        public Block PropertyBlock(List<KeyValuePair<string, Block>> Properties)
        {
            GridBlock gb = new GridBlock(2, Properties.Count);
            gb.Seperator = new Border(0.02, Color.RGB(0.2, 0.2, 0.2));
            for (int t = 0; t < Properties.Count; t++)
            {
                var prop = Properties[t];
                gb[0, t] = this.TextBlock(prop.Key);
                gb[1, t] = prop.Value;
            }
            return gb;
        }
    }
}