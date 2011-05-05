using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.GUI
{
    /// <summary>
    /// A visual representation of a string.
    /// </summary>
    public abstract class Text : Figure
    {
        /// <summary>
        /// Gets the string this text is for.
        /// </summary>
        public abstract string String { get; }

        /// <summary>
        /// Gets the font used to draw this text.
        /// </summary>
        public abstract Font Font { get; }
    }
}