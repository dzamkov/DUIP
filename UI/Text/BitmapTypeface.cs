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
        /// <param name="Size">The edge-length in pixels, of the created bitmap. This should be a power of two.</param>
        /// <param name="CharacterPadding">The amount of padding, in pixels, to put around characters in the texture.</param>
        public static Disposable<BitmapTypeface> Create(
            FontFamily Family, IEnumerable<char> Characters, FontStyle Style, int CharacterPadding,
            float FontSize, int Size)
        {
            var bitmapformat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            using (Bitmap bm = new Bitmap(Size, Size, bitmapformat))
            {
                using (var font = new System.Drawing.Font(Family, FontSize, Style, GraphicsUnit.Pixel))
                {
                    Dictionary<char, Glyph> glyphmap;
                    using (var g = Graphics.FromImage(bm))
                    {
                        g.Clear(Color.Black);

                        StringFormat sf = new StringFormat();
                        sf.Trimming = StringTrimming.None;
                        sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                        sf.FormatFlags |= StringFormatFlags.FitBlackBox;

                        // Create source glyphs
                        IEnumerable<SourceGlyph> glyphs = CreateSourceGlyphs(
                            g, font, sf, Color.Black, Brushes.White, bitmapformat, CharacterPadding, Characters);

                        // Fit
                        double isize = 1.0 / Size;
                        if (!FitSourceGlyphs(bm, g, glyphs, out glyphmap, isize))
                        {
                            return null;
                        }

                        // Dispose glyphs
                        foreach (SourceGlyph gly in glyphs)
                        {
                            gly.Image.Dispose();
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
                    Texture tex = Texture.Create(bd, Texture.Format.A8, false);

                    // Create mipmaps (using sinc filter to give more sharpness than the default box filter).
                    unsafe
                    {
                        byte* source = (byte*)bd.Scan0.ToPointer();
                        byte* temp = source + (Size * Size);
                        byte* dest = temp + (Size * Size / 2);
                        int level = 1;
                        int tsize = Size;
                        while (tsize > 0)
                        {
                            int nsize = tsize / 2;
                            _Downsample(source, temp, dest, tsize);
                            Texture.SetImage(Texture.Format.A8, level, nsize, nsize, new IntPtr(dest));

                            // Swap source and destination buffers
                            byte* t = source;
                            source = dest;
                            dest = t;

                            level++;
                            tsize = nsize;
                        }
                    }


                    bm.UnlockBits(bd);

                    Texture.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
                    Texture.SetWrapMode(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
                    return new BitmapTypeface(tex, glyphmap, (double)Size / (double)FontSize);
                }
            }
        }

        /// <summary>
        /// Downsamples the square bitmap in Source by a factor of 4 (2 on each axis) and outputs the result to Dest. Requires the use of a temporary
        /// buffer with a size half that of the source buffer.
        /// </summary>
        private static unsafe void _Downsample(byte* Source, byte* Temp, byte* Dest, int Size)
        {
            double[] filter = new double[6];

            // Create filter
            double len = filter.Length;
            double tot = 0.0;
            for (int t = 0; t < filter.Length; t++)
            {
                double par = Math.PI * (t * 0.5 + 0.25);
                double val = len * Math.Sin(par) * Math.Sin(par / len) / (par * par);
                filter[t] = val;
                tot += val;
            }

            // Normalize
            double mult = 0.5 / tot;
            for (int t = 0; t < filter.Length; t++)
            {
                filter[t] *= mult;
            }


            int hsize = Size / 2;

            // Horizontal filter from Source to Temp
            for (int y = 0; y < Size; y++)
            {
                byte* sourcescan = Source + (Size * y);
                byte* destscan = Temp + (hsize * y);
                for (int x = 0; x < hsize; x++)
                {
                    tot = 0.0;
                    int rx = x * 2;
                    for (int r = 0; r < filter.Length; r++)
                    {
                        double f = filter[r];
                        int sx = rx - r;
                        int ex = rx + r + 1;
                        if (sx >= 0)
                        {
                            tot += sourcescan[sx] * f;
                        }
                        if (ex < Size)
                        {
                            tot += sourcescan[ex] * f;
                        }
                    }
                    byte res = (byte)Math.Min(Math.Max(tot, 0.0), 255.0);
                    destscan[x] = res;
                }
            }

            // Vertical filter from Temp to Dest
            for (int x = 0; x < hsize; x++)
            {
                byte* sourceoff = Temp + x;
                byte* destoff = Dest + x;
                for (int y = 0; y < hsize; y++)
                {
                    tot = 0.0;
                    int ry = y * 2;
                    for (int r = 0; r < filter.Length; r++)
                    {
                        double f = filter[r];
                        int sy = ry - r;
                        int ey = ry + r + 1;
                        if (sy >= 0)
                        {
                            tot += sourceoff[sy * hsize] * f;
                        }
                        if (ey < Size)
                        {
                            tot += sourceoff[ey * hsize] * f;
                        }
                    }
                    byte res = (byte)Math.Min(Math.Max(tot, 0.0), 255.0);
                    destoff[y * hsize] = res;
                }
            }
        }

        /// <summary>
        /// Describes a glyph within an image to be used as part of a bitmap typeface.
        /// </summary>
        public class SourceGlyph
        {
            /// <summary>
            /// The name of the character this glyph is for.
            /// </summary>
            public char Name;

            /// <summary>
            /// The image this glyph is from.
            /// </summary>
            public Image Image;

            /// <summary>
            /// The x-offset of the glyph in the image.
            /// </summary>
            public int X;

            /// <summary>
            /// The y-offset of the glyph in the image.
            /// </summary>
            public int Y;

            /// <summary>
            /// The width of the glyph in the image.
            /// </summary>
            public int Width;
            
            /// <summary>
            /// The height of the glyph in the image.
            /// </summary>
            public int Height;

            /// <summary>
            /// The offset of the topleft corner of layout rectangle from the topleft corner of the glyph in pixels.
            /// </summary>
            public Point LayoutOffset;

            /// <summary>
            /// The size of the layout rectangle in pixels.
            /// </summary>
            public Point LayoutSize;
        }

        /// <summary>
        /// Creates source glyphs for a gdi+ font. Note that the images for all glyphs need to be disposed manually
        /// after they are no longer in use.
        /// </summary>
        /// <param name="MeasureGraphics">A graphics context used to take initial measurements of characters.</param>
        /// <param name="Padding">The length of uncolored pixels to add around each side of each character.</param>
        public static IEnumerable<SourceGlyph> CreateSourceGlyphs(
            Graphics MeasureGraphics, System.Drawing.Font Font, StringFormat Format,
            Color BackColor, Brush GlyphBrush,
            System.Drawing.Imaging.PixelFormat ImageFormat,
            int Padding, IEnumerable<char> Characters)
        {
            Format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, 1) });
            foreach (char c in Characters)
            {
                // Initial measurements
                string str = char.ToString(c);
                Point fullsize = MeasureGraphics.MeasureString(str, Font, new PointF(0.0f, 0.0f), Format);
                fullsize = fullsize.Ceiling;
                Region[] rgs = MeasureGraphics.MeasureCharacterRanges(str, Font, new RectangleF(Point.Origin, fullsize), Format);
                RectangleF crg = rgs[0].GetBounds(MeasureGraphics);
                Point layoutsize = crg.Size;
                Point layoutoffset = crg.Location;

                // Draw character
                Bitmap bmp = new Bitmap((int)fullsize.X, (int)fullsize.Y, ImageFormat);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(BackColor);
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.DrawString(str, Font, GlyphBrush, 0.0f, 0.0f, Format);
                }

                // Character actual size
                int minx, miny, maxx, maxy;
                _MeasureExtent(bmp, out minx, out miny, out maxx, out maxy);
                minx -= Padding; miny -= Padding; maxx += Padding; maxy += Padding;
                layoutoffset -= new Point(minx, miny);

                // Output glyph
                yield return new SourceGlyph
                {
                    Name = c,
                    Image = bmp,
                    X = minx,
                    Y = miny,
                    Width = maxx - minx,
                    Height = maxy - miny,
                    LayoutOffset = layoutoffset,
                    LayoutSize = layoutsize
                };
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
        /// Tries fitting a collection of source glyphs into a bitmap. Produces a glyphmap if successful.
        /// </summary>
        /// <param name="Scale">The scaling to apply to get from pixel coordinates to glyphmap coordinates.</param>
        public static bool FitSourceGlyphs(
            Bitmap Bitmap, Graphics Graphics, IEnumerable<SourceGlyph> Glyphs,
            out Dictionary<char, Glyph> GlyphMap, double Scale)
        {
            // Sort glyphs by ascending height for increased packing efficency.
            Glyphs = from gly in Glyphs
                     orderby gly.Height ascending
                     select gly;

            // Begin placing glyphs in rows on the bitmap
            GlyphMap = new Dictionary<char, Glyph>();
            int cury = 0;
            int nexty = 0;
            int nextx = 0;
            foreach (SourceGlyph gly in Glyphs)
            {
                int glyx = nextx;
                int glyy = cury;

                nextx = glyx + gly.Width;
                nexty = Math.Max(glyy + gly.Height, nexty);
                if (nextx >= Bitmap.Width)
                {
                    glyx = 0;
                    cury = glyy = nexty;
                    nextx = gly.Width;
                    nexty = glyy + gly.Height;
                }
                if (nexty >= Bitmap.Height)
                {
                    // Nope :(
                    return false;
                }

                Graphics.SetClip(new System.Drawing.Rectangle(glyx, glyy, gly.Width, gly.Height));
                Graphics.DrawImageUnscaled(gly.Image, glyx - gly.X, glyy - gly.Y);
                GlyphMap.Add(gly.Name, new Glyph
                {
                    Source = Rectangle.FromOffsetSize(
                        new Point(glyx, glyy) * Scale, 
                        new Point(gly.Width, gly.Height) * Scale),
                    LayoutOffset = gly.LayoutOffset * Scale,
                    LayoutSize = gly.LayoutSize * Scale
                });
            }
            return true;
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