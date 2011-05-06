using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.GUI
{
    /// <summary>
    /// A collection of glyphs for characters.
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Gets the glyph for the given character, or returns null if the character is not included in this typeset.
        /// </summary>
        public abstract Figure GetGlyph(char Char);

        /// <summary>
        /// Gets the size of the given character for spacing and alignment purposes.
        /// </summary>
        public abstract Point GetSize(char Char);

        /// <summary>
        /// Gets the characters included in this typeset.
        /// </summary>
        public virtual IEnumerable<char> Characters
        {
            get
            {
                ushort t = 0;
                do
                {
                    char c = (char)t;
                    if (this.GetGlyph(c) != null)
                    {
                        yield return c;
                    }
                    t++;
                }
                while (t != 0);
            }
        }

        /// <summary>
        /// Gets an enumerator for all displayable standard ASCII characters.
        /// </summary>
        public static IEnumerable<char> ASCIICharacters
        {
            get
            {
                for (int t = 32; t <= 126; t++)
                {
                    yield return (char)t;
                }
            }
        }
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

        /// <summary>
        /// A text direction where characters progress to the left and lines progress downward.
        /// </summary>
        public static readonly TextDirection LeftDown = new TextDirection()
        {
            MinorDirection = new Point(1.0, 0.0),
            MajorDirection = new Point(0.0, 1.0)
        };

        /// <summary>
        /// A text direction where characters progress to the right and lines progress downward.
        /// </summary>
        public static readonly TextDirection RightDown = new TextDirection()
        {
            MinorDirection = new Point(-1.0, 0.0),
            MajorDirection = new Point(0.0, 1.0)
        };

        /// <summary>
        /// A text direction where characters progress downward and lines progress the the left.
        /// </summary>
        public static readonly TextDirection DownLeft = new TextDirection()
        {
            MinorDirection = new Point(0.0, 1.0),
            MajorDirection = new Point(1.0, 0.0)
        };

        /// <summary>
        /// A text direction where characters progress downward and lines progress the the right.
        /// </summary>
        public static readonly TextDirection DownRight = new TextDirection()
        {
            MinorDirection = new Point(0.0, 1.0),
            MajorDirection = new Point(-1.0, 0.0)
        };
    }
}