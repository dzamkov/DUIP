using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A typeface created from a named font family installed on the system. Actual fonts created 
    /// from this typeface include size and color information.
    /// </summary>
    public abstract class SystemTypeface
    {
        public SystemTypeface(string Name, bool Bold, bool Italic)
        {
            this._Name = Name;
            this._Bold = Bold;
            this._Italic = Italic;
        }

        /// <summary>
        /// Creates a new typeface with the given properties.
        /// </summary>
        public static SystemTypefaceConstructor Create;

        /// <summary>
        /// Gets the name of this typeface.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        /// <summary>
        /// Gets if this typeface is bolded.
        /// </summary>
        public bool Bold
        {
            get
            {
                return this._Bold;
            }
        }

        /// <summary>
        /// Gets if this typeface is italicized.
        /// </summary>
        public bool Italic
        {
            get
            {
                return this._Italic;
            }
        }

        /// <summary>
        /// Gets a font using this typeface.
        /// </summary>
        public SystemFont GetFont(double Size, Color Color)
        {
            return new SystemFont(this, Size, Color);
        }

        /// <summary>
        /// Gets the size of a character in this typeface when using a unit font size.
        /// </summary>
        public abstract Point GetSize(char Char);

        private string _Name;
        private bool _Bold;
        private bool _Italic;
    }

    /// <summary>
    /// A constructor for a system typeface.
    /// </summary>
    /// <remarks>All typeface's constructed with the same parameters are functionally equivalent, however,
    /// the optimal methods for accessing and managing the typeface may vary.</remarks>
    public delegate SystemTypeface SystemTypefaceConstructor(string Name, bool Bold, bool Italic);

    /// <summary>
    /// A font that uses a system typeface.
    /// </summary>
    public sealed class SystemFont : Font
    {
        public SystemFont(SystemTypeface Typeface, double Size, Color Color)
        {
            this._Typeface = Typeface;
            this._Size = Size;
            this._Color = Color;
        }

        public SystemFont(string Name, bool Bold, bool Italic, double Size, Color Color)
            : this(SystemTypeface.Create(Name, Bold, Italic), Size, Color)
        {

        }

        public SystemFont(string Name, double Size, Color Color)
            : this(Name, false, false, Size, Color)
        {

        }

        /// <summary>
        /// Gets the typeface for this font.
        /// </summary>
        public SystemTypeface Typeface
        {
            get
            {
                return this._Typeface;
            }
        }

        /// <summary>
        /// Gets the size of this font. This should be around the height of a
        /// glyph for a capital character.
        /// </summary>
        public double Size
        {
            get
            {
                return this._Size;
            }
        }

        /// <summary>
        /// Gets the color of this font.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        public override Figure GetGlyph(char Char)
        {
            return new SystemFontGlyph(this, Char);
        }

        public override Point GetSize(char Char)
        {
            return this._Typeface.GetSize(Char) * this._Size;
        }

        private SystemTypeface _Typeface;
        private double _Size;
        private Color _Color;
    }

    /// <summary>
    /// A glyph for a system font.
    /// </summary>
    public sealed class SystemFontGlyph : Figure
    {
        public SystemFontGlyph(SystemFont Font, char Character)
        {
            this._Font = Font;
            this._Character = Character;
        }

        /// <summary>
        /// Gets the font for this glyph.
        /// </summary>
        public SystemFont Font
        {
            get
            {
                return this._Font;
            }
        }

        /// <summary>
        /// Gets the character this glyph is for.
        /// </summary>
        public char Character
        {
            get
            {
                return this._Character;
            }
        }

        private SystemFont _Font;
        private char _Character;
    }
}