using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Represents a glyph of a certain character at a certain location.
    /// </summary>
    public struct Character
    {
        public Character(char Name, Point Position)
        {
            this.Name = Name;
            this.Position = Position;
        }

        /// <summary>
        /// The name of the character.
        /// </summary>
        public char Name;

        /// <summary>
        /// The position of the top-left corner of the glyp for the character.
        /// </summary>
        public Point Position;
    }
}