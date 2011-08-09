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
            this.Name = Name;
            this.Bold = Bold;
            this.Italic = Italic;
        }

        /// <summary>
        /// Creates a new typeface with the given properties.
        /// </summary>
        public static SystemTypefaceConstructor Create;

        /// <summary>
        /// The name of this typeface.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Indicates wether this typeface is bolded.
        /// </summary>
        public readonly bool Bold;

        /// <summary>
        /// Indicates wether this typeface is italicized.
        /// </summary>
        public readonly bool Italic;
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
            this.Typeface = Typeface;
            this.Size = Size;
            this.Color = Color;
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
        /// The typeface for this font.
        /// </summary>
        public readonly SystemTypeface Typeface;

        /// <summary>
        /// The size of this font. This should be around the height of a
        /// glyph for a capital character.
        /// </summary>
        public readonly double Size;

        /// <summary>
        /// The color of this font.
        /// </summary>
        public readonly Color Color;

        public override Figure GetGlyph(char Char)
        {
            return new SystemFontGlyph(this, Char);
        }

        public override Point GetSize(char Char)
        {
            return this.Typeface.GetSize(Char) * this.Size;
        }
    }

    /// <summary>
    /// A glyph for a system font.
    /// </summary>
    public sealed class SystemFontGlyph : Figure
    {
        public SystemFontGlyph(SystemFont Font, char Character)
        {
            this.Font = Font;
            this.Character = Character;
        }

        /// <summary>
        /// The font for this glyph.
        /// </summary>
        public readonly SystemFont Font;

        /// <summary>
        /// The character this glyph is for.
        /// </summary>
        public readonly char Character;
    }
}