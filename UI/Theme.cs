using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Defines user and context dependant information used to display items and content.
    /// </summary>
    public class Theme
    {
        public Theme()
        {
            this._DefaultTypeface = BitmapTypeface.Create(BitmapTypeface.GetFamily("Arial"), Font.ASCIICharacters, System.Drawing.FontStyle.Regular, 8, 45.0f, 512);
        }

        /// <summary>
        /// Gets a font suited for the given purpose.
        /// </summary>
        public Font GetFont(FontPurpose Purpose)
        {
            return this._DefaultTypeface.GetFont(0.05, Color.RGB(0.0, 0.0, 0.0));
        }

        /// <summary>
        /// Gets the border and background style of a node with the given parameters.
        /// </summary>
        public void GetNodeStyle(out Border Border, out Color Background)
        {
            Border = new Border(0.04, Color.RGB(0.2, 0.2, 0.2));
            Background = Color.RGB(1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Gets the default flowstyle for the theme.
        /// </summary>
        public FlowStyle FlowStyle
        {
            get
            {
                return new FlowStyle
                {
                    Direction = FlowDirection.RightDown,
                    Justification = FlowJustification.Justify,
                    LineAlignment = Alignment.Center,
                    LineSpacing = 0.00,
                    LineSize = 0.01
                };
            }
        }

        /// <summary>
        /// Creates a block that displays general-purpose text.
        /// </summary>
        public Block TextBlock(string Text)
        {
            return new FlowBlock
            {
                Fit = FlowFit.AspectRatio(2.0),
                Style = this.FlowStyle,
                Items = FlowItem.CreateText(Text, this.GetFont(FontPurpose.General), 0.01, true)
            }.WithPad(0.04);
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

        private BitmapTypeface _DefaultTypeface;
    }

    /// <summary>
    /// A possible use for a font.
    /// </summary>
    public enum FontPurpose
    {
        General,
        Key,
        Value
    }
}