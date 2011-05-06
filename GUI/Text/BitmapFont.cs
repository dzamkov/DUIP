using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// A font that takes characters from a single bitmap image.
    /// </summary>
    public class BitmapFont : Font, IDisposable
    {
        public BitmapFont(Bitmap Bitmap, Dictionary<char, Glyph> GlyphMap, double Scale)
        {
            this._Bitmap = Bitmap;
            this._GlyphMap = GlyphMap;
            this._Scale = Scale;
        }

        /// <summary>
        /// Gets the glyph map (which gives the location of glyphs for characters in a bitmap) for the bitmap font.
        /// </summary>
        public Dictionary<char, Glyph> GlyphMap
        {
            get
            {
                return this._GlyphMap;
            }
        }

        /// <summary>
        /// Gets the bitmap for the bitmap font.
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                return this._Bitmap;
            }
        }

        /// <summary>
        /// Gets a texture for the bitmap font.
        /// </summary>
        public Texture Texture
        {
            get
            {
                if (this._Texture.IsNull)
                {
                    this._Texture = Texture.Create(this._Bitmap);
                    Texture.SetWrapMode(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
                }
                return this._Texture;
            }
        }

        /// <summary>
        /// Gets the amount each character is scaled by when used in relation to the size of the bitmap.
        /// </summary>
        public double Scale
        {
            get
            {
                return this._Scale;
            }
        }

        /// <summary>
        /// Creates a bitmap font for a certain arrangement of the given font family. Returns null if a bitmap font with the requested parameters can not be created.
        /// </summary>
        /// <param name="Characters">The characters to include in the bitmap font.</param>
        /// <param name="Scale">The scale (em size) to use for the font in the bitmap. This affects the quality of the displayed characters and the
        /// amount of bitmap space needed.</param>
        /// <param name="Size">The edge-length in pixels, of the created bitmap.</param>
        /// <param name="DisplayScale">The scale to use for displayed characters.</param>
        public static BitmapFont Create(
            FontFamily Family, IEnumerable<char> Characters, FontStyle Style, 
            float FontScale, double DisplayScale, TextDirection TextDirection, int Size)
        {
            using (var font = new System.Drawing.Font(Family, FontScale, Style))
            {
                Bitmap bm = new Bitmap(Size, Size);
                Dictionary<char, Glyph> glyphmap = new Dictionary<char, Glyph>();
                using (var g = Graphics.FromImage(bm))
                {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    StringFormat sf = new StringFormat();
                    sf.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, 1) });
                    sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                    int x = 0;
                    int y = 0;
                    int sy = 0;
                    Brush b = Brushes.Black;
                    foreach(char c in Characters)
                    {
                        string str = char.ToString(c);
                        Region[] rgs = g.MeasureCharacterRanges(str, font, new RectangleF(0.0f, 0.0f, Size, Size), sf);
                        RectangleF crg = rgs[0].GetBounds(g);
                        SizeF size = crg.Size;
                        int width = (int)Math.Ceiling(size.Width);
                        int height = (int)Math.Ceiling(size.Height);

                        int cx = x;
                        x = cx + width;
                        if (x >= Size)
                        {
                            cx = 0;
                            x = width;
                            sy = y;
                        }

                        int cy = sy;
                        if (cy + height > y)
                        {
                            y = cy + height;
                        }
                        if (y >= Size)
                        {
                            bm.Dispose();
                            return null;
                        }

                        glyphmap.Add(c, new Glyph()
                        {
                            X = x,
                            Y = y,
                            Width = width,
                            Height = height
                        });
                        g.DrawString(str, font, b, cx, cy, sf);
                    }
                }

                double scale = DisplayScale / FontScale * Size;
                return new BitmapFont(bm, glyphmap, scale);
            }
        }

        /// <summary>
        /// Gets the font family with the given name, or returns null if the font family
        /// can not be found.
        /// </summary>
        public static FontFamily GetFamily(string Name)
        {
            foreach (FontFamily ff in FontFamily.Families)
            {
                if (ff.Name == Name)
                {
                    return ff;
                }
            }
            return null;
        }

        public override Point GetSize(char Char)
        {
            Glyph c;
            if (this._GlyphMap.TryGetValue(Char, out c))
            {
                return new Point(c.Width, c.Height) * this._Scale;
            }
            else
            {
                return new Point(0.0, 0.0);
            }
        }

        public override Figure GetGlyph(char Char)
        {
            return null;
        }

        public void Dispose()
        {
            if (this._Bitmap != null)
            {
                this._Bitmap.Dispose();
            }
            if (!this._Texture.IsNull)
            {
                this._Texture.Dispose();
            }
        }

        /// <summary>
        /// Information about a character within the font.
        /// </summary>
        public class Glyph
        {
            /// <summary>
            /// The x-offset of the leftmost pixel of the character.
            /// </summary>
            public int X;

            /// <summary>
            /// The y-offset of the topmost pixel of the character.
            /// </summary>
            public int Y;

            /// <summary>
            /// The width of the character in pixels.
            /// </summary>
            public int Width;

            /// <summary>
            /// The height of the character in pixels.
            /// </summary>
            public int Height;
        }

        private double _Scale;
        private double _LineSpacing;
        private Bitmap _Bitmap;
        private Texture _Texture;
        private TextDirection _TextDirection;
        private Dictionary<char, Glyph> _GlyphMap;
    }
}