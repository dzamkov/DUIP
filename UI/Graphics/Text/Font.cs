using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A collection of fixed-size, colored glyphs for a subset of visible characters.
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Gets the glyph for the given character, or returns null if the character is not included in this font. When used, the glyph
        /// will be translated so that the origin is at the top left corner of the layout rectangle. It is possible for part of a glyph
        /// to be outside the layout rectangle.
        /// </summary>
        public abstract Figure GetGlyph(char Char);

        /// <summary>
        /// Gets the size of the layout rectangle for the given character for use in spacing and alignment purposes. If this font
        /// does not include the character, a size of (0.0, 0.0) is returned.
        /// </summary>
        public abstract Point GetSize(char Char);
    }
}