using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.UI
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

    /// <summary>
    /// Gives spacing and alignment information for a section of text.
    /// </summary>
    public struct TextStyle
    {
        /// <summary>
        /// The direction of the text.
        /// </summary>
        public TextDirection Direction;

        /// <summary>
        /// The justification mode used for the text.
        /// </summary>
        public TextJustification Justification;

        /// <summary>
        /// The spacing between lines of text.
        /// </summary>
        public double LineSpacing;
    }

    /// <summary>
    /// Gives a possible justification mode for text.
    /// </summary>
    public enum TextJustification
    {
        /// <summary>
        /// Text in a line is centered and not aligned with either side of the line.
        /// </summary>
        Center,
        
        /// <summary>
        /// Text is aligned to both sides of a line.
        /// </summary>
        Justify,

        /// <summary>
        /// Text is aligned only to the beginning of a line.
        /// </summary>
        Ragged,

        /// <summary>
        /// Text is aligned only to the end of a line.
        /// </summary>
        ReverseRagged,
    }

    /// <summary>
    /// Gives the direction of the flow of text between and within lines.
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