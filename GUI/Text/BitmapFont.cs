using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace DUIP.GUI
{
    /// <summary>
    /// A character font that takes characters from a single bitmap image.
    /// </summary>
    public class BitmapFont : CharacterFont, IDisposable
    {
        public override TextDirection TextDirection
        {
            get { throw new NotImplementedException(); }
        }

        public override double GetCharacterSpacing(char PreChar, char PostChar)
        {
            throw new NotImplementedException();
        }

        public override double LineSpacing
        {
            get { throw new NotImplementedException(); }
        }

        public override Rectangle GetBounds(char Char)
        {
            throw new NotImplementedException();
        }

        public override Figure GetCharacter(char Char)
        {
            throw new NotImplementedException();
        }

        public override Text CreateText(string String, Rectangle Bounds)
        {
            throw new NotImplementedException();
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
        public struct Character
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

            /// <summary>
            /// The bounds of the character when drawn. The source image will be stretched to fit these bounds.
            /// </summary>
            public Rectangle Bounds;
        }

        private Bitmap _Bitmap;
        private Texture _Texture;
        private Dictionary<char, Character> _CharacterMap;
    }
}