﻿using System;
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
    /// A collection of fonts that use a single texture to display glyphs.
    /// </summary>
    public class BitmapTypeface : IDisposable
    {
        public BitmapTypeface(Texture Texture, Dictionary<char, Glyph> GlyphMap, double Scale)
        {
            this._Texture = Texture;
            this._GlyphMap = GlyphMap;
            this._Scale = Scale;
        }

        /// <summary>
        /// Gets the glyph map (which gives the location of glyphs for characters in a bitmap) for the typeface.
        /// </summary>
        public Dictionary<char, Glyph> GlyphMap
        {
            get
            {
                return this._GlyphMap;
            }
        }

        /// <summary>
        /// Gets the texture for the typeset. All fonts of this typeface use this texture.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return this._Texture;
            }
        }

        /// <summary>
        /// Gets the amount glyphs in the texture need to be scaled by to have a relative font size of 1.0. This
        /// is related to the size of the texture and the size of the characters within it.
        /// </summary>
        public double Scale
        {
            get
            {
                return this._Scale;
            }
        }

        /// <summary>
        /// Creates a bitmap typeface for a certain arrangement of the given font family. Returns null if a typeface with the requested parameters can not be created.
        /// </summary>
        /// <param name="Characters">The characters to include in the bitmap font.</param>
        /// <param name="FontSize">The size in pixels to use for the font in the bitmap. This affects the quality of the displayed characters and the
        /// amount of bitmap space needed. This has no relation to the size of displayed glyphs.</param>
        /// <param name="Size">The edge-length in pixels, of the created bitmap.</param>
        /// <param name="CharacterPadding">The amount of padding, in pixels, to put around characters in the texture.</param>
        public static BitmapTypeface Create(
            FontFamily Family, IEnumerable<char> Characters, FontStyle Style, double CharacterPadding,
            float FontSize, int Size)
        {
            using (Bitmap bm = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                double isize = 1.0 / Size;
                using (var font = new System.Drawing.Font(Family, FontSize, Style, GraphicsUnit.Pixel))
                {
                    Dictionary<char, Glyph> glyphmap = new Dictionary<char, Glyph>();
                    using (var g = Graphics.FromImage(bm))
                    {
                        g.Clear(Color.Black);
                        g.TextRenderingHint = TextRenderingHint.AntiAlias;

                        StringFormat sf = new StringFormat();
                        sf.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, 1) });
                        sf.Trimming = StringTrimming.None;
                        sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                        sf.FormatFlags |= StringFormatFlags.FitBlackBox;
                        Point nextfree = new Point(0.0, 0.0);
                        double nextheight = 0.0;
                        Brush brush = Brushes.White;

                        Bitmap measurebitmap = null;
                        Graphics measureg = null;

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

                            // GDI+ lies about required bitmap size, figure it out manually for most compact fit
                            Point drawoffset;
                            {
                                fullsize = fullsize.Ceiling;
                                int bw = (int)fullsize.X;
                                int bh = (int)fullsize.Y;
                                if (measurebitmap == null)
                                {
                                    measurebitmap = new Bitmap(bw, bh, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                    measureg = Graphics.FromImage(measurebitmap);
                                }
                                else
                                {
                                    // Resize the bitmap used for measurement if needed
                                    if (measurebitmap.Width < bw || measurebitmap.Height < bh)
                                    {
                                        measurebitmap.Dispose();
                                        measureg.Dispose();
                                        measurebitmap = new Bitmap(bw, bh, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                        measureg = Graphics.FromImage(measurebitmap);
                                    }
                                }

                                measureg.Clear(Color.Black);
                                measureg.TextRenderingHint = g.TextRenderingHint;
                                measureg.DrawString(str, font, brush, 0.0f, 0.0f, sf);

                                int minx, miny, maxx, maxy;
                                _MeasureExtent(measurebitmap, out minx, out miny, out maxx, out maxy);
                                Rectangle extrect = new Rectangle(minx, miny, maxx, maxy);
                                extrect = extrect.Pad(CharacterPadding);

                                drawoffset = extrect.TopLeft;
                                fullsize = extrect.Size;
                                layoutoffset -= extrect.TopLeft;
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

                            // Draw character
                            g.DrawString(str, font, brush, charpoint - drawoffset, sf);

                            // Add glyphmap entry
                            charpoint += new Point(0.5, 0.5);
                            fullsize -= new Point(1.0, 1.0);
                            glyphmap.Add(c, new Glyph
                            {
                                Source = Rectangle.FromOffsetSize(charpoint * isize, fullsize * isize),
                                LayoutSize = layoutsize * isize,
                                LayoutOffset = layoutoffset * isize
                            });
                        }

                        if (measurebitmap != null)
                        {
                            measurebitmap.Dispose();
                            measureg.Dispose();
                        }
                    }

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
                    return new BitmapTypeface(tex, glyphmap, (double)Size / (double)FontSize);
                }
            }
        }

        /// <summary>
        /// Measures the extent of the colored region in a 24bbp bitmap.
        /// </summary>
        private static unsafe void _MeasureExtent(Bitmap Bitmap, out int MinX, out int MinY, out int MaxX, out int MaxY)
        {
            BitmapData bd = Bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            byte* ptr = (Byte*)bd.Scan0.ToPointer();

            int w = bd.Width;
            int h = bd.Height;
            int s = bd.Stride;

            // Left to right scan
            for (MinX = 0; MinX < w; MinX++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (ptr[(MinX * 3) + y * s] > 0)
                    {
                        goto ltr_end;
                    }
                }
            }
        ltr_end:

            // Right to left scan
            for (MaxX = w - 1; MaxX > MinX; MaxX--)
            {
                for (int y = 0; y < h; y++)
                {
                    if (ptr[(MaxX * 3) + y * s] > 0)
                    {
                        goto rtl_end;
                    }
                }
            }
        rtl_end:

            // Top to bottom scan
            for (MinY = 0; MinY < h; MinY++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (ptr[(x * 3) + MinY * s] > 0)
                    {
                        goto ttb_end;
                    }
                }
            }
        ttb_end:

            // Bottom to top scan
            for (MaxY = h - 1; MaxY > MinY; MaxY--)
            {
                for (int x = 0; x < w; x++)
                {
                    if (ptr[(x * 3) + MaxY * s] > 0)
                    {
                        goto btt_end;
                    }
                }
            }
        btt_end:

            Bitmap.UnlockBits(bd);
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

        /// <summary>
        /// Gets an instance of this typeface for the given size and color.
        /// </summary>
        public BitmapFont GetFont(double Size, Color Color)
        {
            return new BitmapFont(this, this._Scale * Size, Color);
        }

        /// <summary>
        /// Information about a glyph in a bitmap typeface.
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

        private Texture _Texture;
        private Dictionary<char, Glyph> _GlyphMap;
        private double _Scale;
    }
}