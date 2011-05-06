using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.GUI
{
    /// <summary>
    /// A visual representation of a string. Unless otherwise specified, the text is white, allowing for color modulation.
    /// </summary>
    public abstract class Text : Figure
    {
        /// <summary>
        /// An instance of a character within text.
        /// </summary>
        public struct Character
        {
            /// <summary>
            /// The name of the character.
            /// </summary>
            public char Name;

            /// <summary>
            /// The offset of the character in space.
            /// </summary>
            public Point Offset;
        }

        /// <summary>
        /// Gets the string this text is for.
        /// </summary>
        public abstract string String { get; }

        /// <summary>
        /// Gets the font used to draw this text.
        /// </summary>
        public abstract Font Font { get; }
    }
}