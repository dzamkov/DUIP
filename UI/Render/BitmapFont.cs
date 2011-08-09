using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SFont = System.Drawing.Font;
using SGraphics = System.Drawing.Graphics;
using SColor = System.Drawing.Color;
using SImage = System.Drawing.Image;

using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI.Render
{
    /// <summary>
    /// A collection of fonts that use a single texture to display glyphs.
    /// </summary>
    public class BitmapTypeface : SystemTypeface
    {
        public BitmapTypeface(string Name, bool Bold, bool Italic)
            : base(Name, Bold, Italic)
        {
            this.GlyphMap = new Dictionary<char, Glyph>();
            this.FontFamily = GetFamily(Name);
            this._CreateSheet(ASCIICharacters);
        }

        /// <summary>
        /// The glyph map (which gives the location of glyphs for characters in a bitmap) for the typeface.
        /// </summary>
        public readonly Dictionary<char, Glyph> GlyphMap;

        /// <summary>
        /// The font family for this typeface.
        /// </summary>
        public readonly FontFamily FontFamily;

        /// <summary>
        /// Gets all ASCII characters.
        /// </summary>
        public static IEnumerable<char> ASCIICharacters
        {
            get
            {
                for (short t = 32; t <= 126; t++)
                {
                    yield return (char)t;
                }
            }
        }

        /// <summary>
        /// Gets the format to used when drawing glyphs.
        /// </summary>
        private StringFormat _FontFormat
        {
            get
            {
                StringFormat sf = new StringFormat();
                sf.Trimming = StringTrimming.None;
                sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                sf.FormatFlags |= StringFormatFlags.FitBlackBox;
                return sf;
            }
        }

        /// <summary>
        /// Gets the font style to use when drawing glyphs.
        /// </summary>
        private FontStyle _FontStyle
        {
            get
            {
                FontStyle fs = FontStyle.Regular;
                if (this.Bold) fs |= FontStyle.Bold;
                if (this.Italic) fs |= FontStyle.Italic;
                return fs;
            }
        }

        /// <summary>
        /// Gets the font size to use when drawing glyphs to a sheet.
        /// </summary>
        private float _FontSize
        {
            get
            {
                return 45.0f;
            }
        }

        /// <summary>
        /// Gets the amount of padding (in pixels) to apply to glyphs.
        /// </summary>
        private int _GlyphPadding
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Gets the filter usd for creating mipmaps for the typeface.
        /// </summary>
        private double[] _Filter
        {
            get
            {
                return _CreateLanczosFilter(6, 6.0);
            }
        }

        /// <summary>
        /// Gets the size in pixels of a sheet for the typeface.
        /// </summary>
        private int _SheetSize
        {
            get
            {
                return 512;
            }
        }

        /// <summary>
        /// The format used for drawing glyphs into bitmaps before converting them to textures.
        /// </summary>
        private const System.Drawing.Imaging.PixelFormat _BitmapFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

        /// <summary>
        /// Creates a sheet containing the given characters and adds them to the glyphmap.
        /// </summary>
        private Sheet _CreateSheet(IEnumerable<char> Characters)
        {
            int size = this._SheetSize;
            using (Bitmap bm = new Bitmap(size, size, _BitmapFormat))
            {
                float fontsize = this._FontSize;
                using (SFont font = new SFont(this.FontFamily, fontsize, this._FontStyle, GraphicsUnit.Pixel))
                {
                    // Create sheet to be referenced by glyphs
                    Sheet sheet = new Sheet();

                    // Draw glyphs to bitmap
                    using (SGraphics g = SGraphics.FromImage(bm))
                    {
                        g.Clear(SColor.Black);
                        IEnumerable<SourceGlyph> srcglyphs = CreateSourceGlyphs(g, font, this._FontFormat, SColor.Black, Brushes.White, this._GlyphPadding, Characters);
                        FitSourceGlyphs(bm, g, srcglyphs, sheet, 1.0 / size, 1.0 / fontsize, this.GlyphMap);

                        // Don't forget to dispose source glyphs
                        foreach (SourceGlyph gly in srcglyphs)
                        {
                            gly.Image.Dispose();
                        }
                    }

                    // Create texture
                    sheet.Texture = _CreateTexture(bm, size, this._Filter);
                    sheet.Scale = size / fontsize;

                    // All done
                    return sheet;
                }
            }
        }

        public override Point GetSize(char Char)
        {
            Glyph gly = this.GlyphMap[Char];
            return gly.LayoutSize;
        }

        /// <summary>
        /// Gets the glyph information for a character.
        /// </summary>
        public Glyph GetGlyph(char Char)
        {
            return this.GlyphMap[Char];
        }

        /// <summary>
        /// Creates a windowed lanczos filter for downsampling.
        /// </summary>
        private static double[] _CreateLanczosFilter(int Size, double Radius)
        {
            double[] filter = new double[Size];

            // Create filter
            double tot = 0.0;
            for (int t = 0; t < filter.Length; t++)
            {
                double par = Math.PI * (t * 0.5 + 0.25);
                double val = Radius * Math.Sin(par) * Math.Sin(par / Radius) / (par * par);
                filter[t] = val;
                tot += val;
            }

            // Normalize
            double mult = 0.5 / tot;
            for (int t = 0; t < filter.Length; t++)
            {
                filter[t] *= mult;
            }

            return filter;
        }

        /// <summary>
        /// Destructively creates an alpha-only texture for a 24bpp bitmap with a custom mipmap filer.
        /// </summary>
        private static unsafe Texture _CreateTexture(Bitmap Source, int Size, double[] Filter)
        {
            // Convert bitmap to an alpha-only format
            BitmapData bd = Source.LockBits(
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
            Texture tex = Texture.Create();
            Texture.SetImage(Texture.Format.A8, 0, Size, Size, bd.Scan0);
            Texture.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);

            // Create mipmaps (using lancsoz filter to give more sharpness than the default box filter).
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
                    _Downsample(Filter, source, temp, dest, tsize);
                    Texture.SetImage(Texture.Format.A8, level, nsize, nsize, new IntPtr(dest));

                    // Swap source and destination buffers
                    byte* t = source;
                    source = dest;
                    dest = t;

                    level++;
                    tsize = nsize;
                }
            }


            Source.UnlockBits(bd);

            Texture.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            Texture.SetWrapMode(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            return tex;
        }

        /// <summary>
        /// Downsamples the square bitmap in Source by a factor of 4 (2 on each axis) and outputs the result to Dest. Requires the use of a temporary
        /// buffer with a size half that of the source buffer.
        /// </summary>
        private static unsafe void _Downsample(double[] Filter, byte* Source, byte* Temp, byte* Dest, int Size)
        {
          
            int hsize = Size / 2;

            // Horizontal filter from Source to Temp
            for (int y = 0; y < Size; y++)
            {
                byte* sourcescan = Source + (Size * y);
                byte* destscan = Temp + (hsize * y);
                for (int x = 0; x < hsize; x++)
                {
                    double tot = 0.0;
                    int rx = x * 2;
                    for (int r = 0; r < Filter.Length; r++)
                    {
                        double f = Filter[r];
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
                    double tot = 0.0;
                    int ry = y * 2;
                    for (int r = 0; r < Filter.Length; r++)
                    {
                        double f = Filter[r];
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
            public SImage Image;

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
            SGraphics MeasureGraphics, SFont Font, StringFormat Format,
            SColor BackColor, Brush GlyphBrush,
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
                Bitmap bmp = new Bitmap((int)fullsize.X, (int)fullsize.Y, _BitmapFormat);
                using (SGraphics g = SGraphics.FromImage(bmp))
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
        /// <param name="Sheet">The sheet reference to be used for glyphs.</param>
        /// <param name="TextureScale">The size of a pixel in relation to the size of the texture.</param>
        /// <param name="FontScale">The size of a pixel in relation to the size of the font.</param>
        /// <returns>True if all source glyphs fit, false otherwise.</returns>
        public static bool FitSourceGlyphs(
            Bitmap Bitmap, SGraphics Graphics, IEnumerable<SourceGlyph> Glyphs,
            Sheet Sheet, double TextureScale, double FontScale, Dictionary<char, Glyph> GlyphMap)
        {
            // Sort glyphs by ascending height for increased packing efficency.
            Glyphs = from gly in Glyphs
                     orderby gly.Height ascending
                     select gly;

            // Begin placing glyphs in rows on the bitmap
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
                    Sheet = Sheet,
                    Source = Rectangle.FromOffsetSize(
                        new Point(glyx, glyy) * TextureScale, 
                        new Point(gly.Width, gly.Height) * TextureScale),
                    LayoutOffset = gly.LayoutOffset * FontScale,
                    LayoutSize = gly.LayoutSize * FontScale
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
        /// References a texture that contains glyphs.
        /// </summary>
        public class Sheet
        {
            /// <summary>
            /// The texture for this sheet.
            /// </summary>
            public Texture Texture;

            /// <summary>
            /// The size of the texture in relation to the size of the font.
            /// </summary>
            public double Scale;
        }

        /// <summary>
        /// Information about a glyph in a bitmap typeface.
        /// </summary>
        public class Glyph
        {
            /// <summary>
            /// The sheet this glyph is in.
            /// </summary>
            public Sheet Sheet;

            /// <summary>
            /// The location of the glyph within the texture for the font.
            /// </summary>
            public Rectangle Source;

            /// <summary>
            /// The offset of the layout rectangle of the glyph from the topleft corner of the glyph's textured quad
            /// when using a font with a size of 1.0.
            /// </summary>
            public Point LayoutOffset;

            /// <summary>
            /// The size of the layout rectangle of the glyph when using a font with a size of 1.0.
            /// </summary>
            public Point LayoutSize;

            /// <summary>
            /// Gets information needed to render an instance of this glyph using the origin as the top-left corner of the layout
            /// rectangle.
            /// </summary>
            public void GetRenderInfo(double Size, out Texture Texture, out Rectangle Source, out Rectangle Destination)
            {
                Sheet sheet = this.Sheet;
                Texture = sheet.Texture;
                Source = this.Source;
                Destination = Rectangle.FromOffsetSize(
                    -this.LayoutOffset * Size,
                    this.Source.Size * sheet.Scale * Size);
            }
        }

        
    }
}