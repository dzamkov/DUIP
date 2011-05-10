using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A collection of glyphs for characters.
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Gets the glyph for the given character, or returns null if the character is not included in this font. The glyph should
        /// be bounded between the origin and the point corresponding to the size of the character.
        /// </summary>
        public abstract Disposable<Figure> GetGlyph(char Char, Color Color);

        /// <summary>
        /// Gets a text of the given characters using this font.
        /// </summary>
        public virtual Disposable<Figure> GetText(IEnumerable<Character> Characters, Color Color)
        {
            return new _Text(
                from c in Characters
                select this.GetGlyph(c.Name, Color).Object.WithTranslate(c.Position)
            );
        }

        private class _Text : GroupFigure, IDisposable
        {
            public _Text(IEnumerable<Figure> Components)
                : base(Components)
            {
                
            }

            public void Dispose()
            {
                foreach (Figure f in this.Components)
                {
                    ((Disposable<Figure>)f).Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the size of the given character for spacing and alignment purposes. If this font does not include the character,
        /// a size of (0.0, 0.0) is returned.
        /// </summary>
        public abstract Point GetSize(char Char);

        /// <summary>
        /// Gets the characters included in this font.
        /// </summary>
        public virtual IEnumerable<char> Characters
        {
            get
            {
                ushort t = 0;
                do
                {
                    char c = (char)t;
                    using (var gly = this.GetGlyph(c, Color.White))
                    {
                        if (gly.Object != null)
                        {
                            yield return c;
                        }
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
}