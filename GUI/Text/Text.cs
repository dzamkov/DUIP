using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.GUI
{
    /// <summary>
    /// A visual representation of a string.
    /// </summary>
    public abstract class Text
    {
        /// <summary>
        /// Gets the string this text is for.
        /// </summary>
        public abstract string String { get; }

        /// <summary>
        /// Gets the font used to draw this text.
        /// </summary>
        public abstract Font Font { get; }

        /// <summary>
        /// Gets the bounding rectangle for the given character.
        /// </summary>
        public abstract Rectangle GetCharacterBounds(int CharacterIndex);

        /// <summary>
        /// Gets the bounding rectangle for a given substring of the text.
        /// </summary>
        public abstract Rectangle GetSubstringBounds(int StartIndex, int Size);

        /// <summary>
        /// Gets the bounding rectangle for a line in the text.
        /// </summary>
        public virtual Rectangle GetLineBounds(int LineIndex)
        {
            int si, s;
            this.GetLineSubstring(LineIndex, out si, out s);
            return this.GetSubstringBounds(si, s);
        }

        /// <summary>
        /// Gets the substring for a given line index.
        /// </summary>
        public abstract void GetLineSubstring(int LineIndex, out int StartIndex, out int Size);

        /// <summary>
        /// Gets the line the given character is on.
        /// </summary>
        public abstract int GetLine(int CharacterIndex);

        /// <summary>
        /// Renders the text to the current graphics context.
        /// </summary>
        public abstract void Render();
    }
}