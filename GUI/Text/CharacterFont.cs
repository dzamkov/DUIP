using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.GUI
{
    /// <summary>
    /// A font that combines figures for individual characters to form a text.
    /// </summary>
    public abstract class CharacterFont : Font
    {
        /// <summary>
        /// Gets the direction of text with this font.
        /// </summary>
        public abstract TextDirection TextDirection { get; }

        /// <summary>
        /// Gets the amount of space between the two given characters.
        /// </summary>
        public abstract double GetCharacterSpacing(char PreChar, char PostChar);

        /// <summary>
        /// Gets the amount of space between two adjacent lines.
        /// </summary>
        public abstract double LineSpacing { get; }

        /// <summary>
        /// Gets the bounds of a character with the origin being on the midline of a line of text.
        /// </summary>
        public abstract Rectangle GetBounds(char Char);

        /// <summary>
        /// Gets the figure for the given character. The figure should line up with the bounds of the character.
        /// </summary>
        public abstract Figure GetCharacter(char Char);
    }

    /// <summary>
    /// A structure that gives the direction of the flow of text between and within lines.
    /// </summary>
    public struct TextDirection
    {
        /// <summary>
        /// A normal vector that gives the direction in which characters within a line are arranged.
        /// </summary>
        public Point MinorDirection;

        /// <summary>
        /// A normal vector that gives the direction in which direction lines are arranged.
        /// </summary>
        public Point MajorDirection;
    }
}