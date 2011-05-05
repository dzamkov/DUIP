using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.GUI
{
    /// <summary>
    /// Describes a method of drawing and arranging strings in a visual representation.
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Uses this font to create a text sample with the given string, within the given bounds.
        /// </summary>
        public abstract Text CreateText(string String, Rectangle Bounds);
    }
}