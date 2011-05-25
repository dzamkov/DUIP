using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
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
        public abstract Disposable<Figure> GetGlyph(char Char);

        /// <summary>
        /// Gets a drawer for this font.
        /// </summary>
        public virtual Drawer GetDrawer()
        {
            return new DefaultDrawer(this);
        }

        /// <summary>
        /// Draws glyphs from a single font consecutively.
        /// </summary>
        public abstract class Drawer
        {
            /// <summary>
            /// Begins drawing on the given context.
            /// </summary>
            public virtual void Begin(RenderContext Context)
            {

            }

            /// <summary>
            /// Draws a glyph for a character at the given offset.
            /// </summary>
            public virtual void Draw(RenderContext Context, char Char, Point Offset)
            {

            }

            /// <summary>
            /// Ends drawing on the given context.
            /// </summary>
            public virtual void End(RenderContext Context)
            {

            }
        }

        /// <summary>
        /// A drawer for a font used when no other is defined.
        /// </summary>
        public sealed class DefaultDrawer : Drawer
        {
            public DefaultDrawer(Font Font)
            {
                this._Font = Font;
            }

            public override void Draw(RenderContext Context, char Char, Point Offset)
            {
                Disposable<Figure> glyph = this._Font.GetGlyph(Char);
                ((Figure)glyph).WithTranslate(Offset).Render(Context);
                glyph.Dispose();
            }

            private Font _Font;
        }

        /// <summary>
        /// Gets the size of the layout rectangle for the given character for use in spacing and alignment purposes. If this font
        /// does not include the character, a size of (0.0, 0.0) is returned.
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
                    using (var gly = this.GetGlyph(c))
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