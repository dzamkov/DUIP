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
    /// A font that takes glyphs from a single texture image.
    /// </summary>
    public class BitmapFont : Font, IDisposable
    {
        public BitmapFont(Texture Texture, Dictionary<char, Rectangle> GlyphMap, double Scale)
        {
            this._Texture = Texture;
            this._GlyphMap = GlyphMap;
            this._Scale = Scale;
        }

        /// <summary>
        /// Gets the glyph map (which gives the location of glyphs for characters in a bitmap) for the bitmap font.
        /// </summary>
        public Dictionary<char, Rectangle> GlyphMap
        {
            get
            {
                return this._GlyphMap;
            }
        }

        /// <summary>
        /// Gets a texture for the bitmap font.
        /// </summary>
        public Texture Texture
        {
            get
            {
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
            using (Bitmap bm = new Bitmap(Size, Size))
            {
                double isize = 1.0 / Size;
                using (var font = new System.Drawing.Font(Family, FontScale, Style))
                {
                    Dictionary<char, Rectangle> glyphmap = new Dictionary<char, Rectangle>();
                    using (var g = Graphics.FromImage(bm))
                    {
                        g.Clear(Color.Transparent);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        StringFormat sf = new StringFormat();
                        sf.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, 1) });
                        sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                        double x = 0;
                        double y = 0;
                        double sy = 0;
                        Brush b = Brushes.Black;
                        foreach (char c in Characters)
                        {
                            string str = char.ToString(c);
                            Region[] rgs = g.MeasureCharacterRanges(str, font, new RectangleF(0.0f, 0.0f, Size, Size), sf);
                            RectangleF crg = rgs[0].GetBounds(g);
                            SizeF size = crg.Size;
                            double width = crg.Width;
                            double height = crg.Height;

                            double cx = x;
                            x = cx + width;
                            if (x >= Size)
                            {
                                cx = 0;
                                x = width;
                                sy = y;
                            }

                            double cy = sy;
                            if (cy + height > y)
                            {
                                y = cy + height;
                            }
                            if (y >= Size)
                            {
                                bm.Dispose();
                                return null;
                            }

                            glyphmap.Add(c, Rectangle.FromOffsetSize(
                                new Point(cx + 0.5, cy + 0.5) * isize,
                                new Point(width, height) * isize));
                            g.DrawString(str, font, b, (float)cx - crg.X, (float)cy - crg.Y, sf);
                        }
                    }

                    double scale = DisplayScale / FontScale * Size;

                    Texture tex = Texture.Create(bm);
                    Texture.SetWrapMode(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
                    return new BitmapFont(tex, glyphmap, scale);
                }
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
            Rectangle src;
            if (this._GlyphMap.TryGetValue(Char, out src))
            {
                return src.Size * this._Scale;
            }
            return new Point(0.0, 0.0);
        }

        public override Figure GetGlyph(char Char)
        {
            Rectangle src;
            if (this._GlyphMap.TryGetValue(Char, out src))
            {
                return this._Texture.CreateFigure(src, new Rectangle(new Point(0.0, 0.0), src.Size * this._Scale));
            }
            return null;
        }

        public void Dispose()
        {
            this._Texture.Dispose();
        }

        private double _Scale;
        private Texture _Texture;
        private Dictionary<char, Rectangle> _GlyphMap;
    }
}