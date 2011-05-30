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
            Border = new Border
            {
                Color = Color.RGB(0.2, 0.2, 0.2),
                Weight = 0.04,
            };
            Background = Color.RGB(1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Gets the default flow style to use for text.
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
        /// Gets the default amount of padding to apply to a section of text or content.
        /// </summary>
        public double TextPadding
        {
            get
            {
                return 0.05;
            }
        }

        /// <summary>
        /// Gets a block that displays the given text.
        /// </summary>
        public Block GetTextBlock(string Text)
        {
            return Block.Text(Text, this.GetFont(FontPurpose.General), this.FlowStyle).WithPad(this.TextPadding);
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