using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A font of a bitmap typeface with a certain scale and color.
    /// </summary>
    public class BitmapFont : Font
    {
        public BitmapFont(BitmapTypeface Typeface, double Scale, Color Color)
        {
            this._Typeface = Typeface;
            this._Scale = Scale;
            this._Color = Color;
        }

        public override IEnumerable<char> Characters
        {
            get
            {
                return this._Typeface.GlyphMap.Keys;
            }
        }

        public override Figure GetGlyph(char Char)
        {
            BitmapTypeface.Glyph gly;
            if (this._Typeface.GlyphMap.TryGetValue(Char, out gly))
            {
                Rectangle src = gly.Source;
                Rectangle dst = Rectangle.FromOffsetSize(-gly.LayoutOffset * this._Scale, src.Size * this._Scale);
                return this._Typeface.Texture.CreateFigure(src, dst, this._Color);
            }
            return null;
        }

        public override Point GetSize(char Char)
        {
            BitmapTypeface.Glyph gly;
            if (this._Typeface.GlyphMap.TryGetValue(Char, out gly))
            {
                return gly.LayoutSize * this._Scale;
            }
            return Point.Zero;
        }

        private BitmapTypeface _Typeface;
        private double _Scale;
        private Color _Color;
    }
}