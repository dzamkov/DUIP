using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
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

        public override Font.Drawer CreateDrawer(RenderContext Context)
        {
            return new Drawer(Context, this);
        }

        /// <summary>
        /// A drawer for a bitmap font.
        /// </summary>
        public new class Drawer : Font.Drawer
        {
            public Drawer(RenderContext Context, BitmapFont Font)
            {
                Context.SetColor(Font._Color);
                Context.SetTexture(Font._Typeface.Texture);
                Context.DrawQuads();
                this._Font = Font;
            }

            public override void Draw(RenderContext Context, char Char, Point Offset)
            {
                double scale = this._Font._Scale;
                BitmapTypeface.Glyph gly;
                if (this._Font._Typeface.GlyphMap.TryGetValue(Char, out gly))
                {
                    Rectangle src = gly.Source;
                    Rectangle dst = Rectangle.FromOffsetSize(-gly.LayoutOffset * scale + Offset, src.Size * scale);
                    Context.OutputTexturedQuad(src, dst);
                }
            }

            public override Font.Drawer Switch(RenderContext Context, Font Font)
            {
                BitmapFont ofont = Font as BitmapFont;
                if (ofont != null)
                {
                    if (ofont._Typeface == this._Font._Typeface)
                    {
                        this._Font = ofont;
                        Context.SetColor(ofont._Color);
                        return this;
                    }
                }

                return base.Switch(Context, Font);
            }

            public override void End(RenderContext Context)
            {
                Context.Pop();
            }

            private BitmapFont _Font;
        }

        public override IEnumerable<char> Characters
        {
            get
            {
                return this._Typeface.GlyphMap.Keys;
            }
        }

        public override Disposable<Figure> GetGlyph(char Char)
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