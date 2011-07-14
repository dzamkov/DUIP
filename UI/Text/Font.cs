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
        /// Creates and start a drawer for this font. The context given should not be used until end is called on the corresponding drawer.
        /// </summary>
        public virtual Drawer CreateDrawer(RenderContext Context)
        {
            return new DefaultDrawer(this);
        }

        /// <summary>
        /// Draws glyphs from a single font consecutively.
        /// </summary>
        public abstract class Drawer
        {
            /// <summary>
            /// Draws a glyph for a character at the given offset.
            /// </summary>
            public virtual void Draw(RenderContext Context, char Char, Point Offset)
            {

            }

            /// <summary>
            /// Switches the font used. If this is called, it should be called in place of End.
            /// </summary>
            public virtual Drawer Switch(RenderContext Context, Font Font)
            {
                this.End(Context);
                return Font.CreateDrawer(Context);
            }

            /// <summary>
            /// Ends drawing on the given context.
            /// </summary>
            public virtual void End(RenderContext Context)
            {

            }
        }

        /// <summary>
        /// Draws glyphs from a selectable font consecutively.
        /// </summary>
        public struct MultiDrawer
        {
            /// <summary>
            /// Gets the font this drawer uses.
            /// </summary>
            public Font Font
            {
                get
                {
                    return this._Font;
                }
            }

            /// <summary>
            /// Sets the font this drawer uses.
            /// </summary>
            public void Select(RenderContext Context, Font Font)
            {
                this._Font = Font;
                if (this._Drawer != null)
                {
                    this._Drawer.Switch(Context, Font);
                }
            }

            /// <summary>
            /// Draws a glyph for a character at the given offset.
            /// </summary>
            public void Draw(RenderContext Context, char Char, Point Offset)
            {
                if (this._Drawer == null)
                {
                    this._Drawer = this._Font.CreateDrawer(Context);
                }
                this._Drawer.Draw(Context, Char, Offset);
            }

            /// <summary>
            /// Temporarily ends font drawing operations to allow the render context to be used. This should also be called
            /// when the drawer will no longer be used.
            /// </summary>
            public void Flush(RenderContext Context)
            {
                if (this._Drawer != null)
                {
                    this._Drawer.End(Context);
                    this._Drawer = null;
                }
            }

            private Font _Font;
            private Drawer _Drawer;
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