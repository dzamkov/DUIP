using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// A font that takes glyphs from a single texture image.
    /// </summary>
    public class BitmapFont : Font, IDisposable
    {
        public BitmapFont(Texture Texture, Dictionary<char, Glyph> GlyphMap, double Scale)
        {
            this._Texture = Texture;
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
            float FontScale, double DisplayScale, int Size)
        {
            using (Bitmap bm = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                double isize = 1.0 / Size;
                using (var font = new System.Drawing.Font(Family, FontScale, Style))
                {
                    Dictionary<char, Glyph> glyphmap = new Dictionary<char, Glyph>();
                    using (var g = Graphics.FromImage(bm))
                    {
                        g.Clear(Color.Black);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        StringFormat sf = new StringFormat();
                        sf.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, 1) });
                        sf.Trimming = StringTrimming.None;
                        sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                        sf.FormatFlags |= StringFormatFlags.FitBlackBox;
                        Point nextfree = new Point(0.0, 0.0);
                        double nextheight = 0.0;
                        Brush brush = Brushes.White;
                        foreach (char c in Characters)
                        {
                            // Character measurements
                            string str = char.ToString(c);
                            Point fullsize, layoutsize, layoutoffset;
                            {
                                fullsize = g.MeasureString(str, font, Size, sf);
                                Region[] rgs = g.MeasureCharacterRanges(str, font, new RectangleF(Point.Origin, fullsize), sf);
                                RectangleF crg = rgs[0].GetBounds(g);
                                layoutsize = crg.Size;
                                layoutoffset = crg.Location;
                            }

                            // Fit character in bitmap
                            Point charpoint = nextfree;
                            nextheight = Math.Max(nextheight, charpoint.Y + fullsize.Y);
                            nextfree.X = charpoint.X + fullsize.X;
                            if (nextfree.X >= Size)
                            {
                                charpoint.X = 0.0;
                                nextfree.X = fullsize.X;
                                charpoint.Y = nextheight;
                                nextfree.Y = nextheight;
                            }
                            if (nextheight >= Size)
                            {
                                bm.Dispose();
                                return null;
                            }

                            // Add glyphmap entry
                            glyphmap.Add(c, new Glyph
                            {
                                Source = Rectangle.FromOffsetSize(charpoint * isize, fullsize * isize),
                                LayoutSize = layoutsize * isize,
                                LayoutOffset = layoutoffset * isize
                            });

                            // Draw character
                            g.DrawString(str, font, brush, charpoint, sf);
                        }
                    }

                    double scale = DisplayScale / FontScale * Size;

                    // Convert bitmap to an alpha-only format
                    BitmapData bd = bm.LockBits(
                        new System.Drawing.Rectangle(0, 0, Size, Size), 
                        ImageLockMode.ReadWrite, 
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    unsafe
                    {
                        byte* r = (byte*)bd.Scan0.ToPointer();
                        byte* w = r;
                        int tot = Size * Size;
                        for (int t = 0; t < tot; t++)
                        {
                            *w = *r;
                            w += 1;
                            r += 3;
                        }
                    }

                    // Create texture
                    Texture tex = Texture.Create(bd, Texture.Format.A8, true);
                    bm.UnlockBits(bd);

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
            Glyph gly;
            if (this._GlyphMap.TryGetValue(Char, out gly))
            {
                return gly.LayoutSize * this._Scale;
            }
            return new Point(0.0, 0.0);
        }

        public override Disposable<Figure> GetGlyph(char Char, Color Color)
        {
            Glyph gly;
            if (this._GlyphMap.TryGetValue(Char, out gly))
            {
                Rectangle src = gly.Source;
                Point offset = gly.LayoutOffset * this._Scale;
                Rectangle dst = new Rectangle(-offset, src.Size * this._Scale - offset);
                return this._Texture.CreateFigure(src, dst, Color);
            }
            return null;
        }

        /// <summary>
        /// Information about a glyph in a bitmap font.
        /// </summary>
        public class Glyph
        {
            /// <summary>
            /// The location of the glyph within the texture for the font.
            /// </summary>
            public Rectangle Source;

            /// <summary>
            /// The offset of the layout rectangle of the glyph from the topleft corner of the source rectangle.
            /// </summary>
            public Point LayoutOffset;

            /// <summary>
            /// The size of the layout rectangle of the glyph before scaling.
            /// </summary>
            public Point LayoutSize;
        }

        public void Dispose()
        {
            this._Texture.Dispose();
        }

        private double _Scale;
        private Texture _Texture;
        private Dictionary<char, Glyph> _GlyphMap;
    }
}