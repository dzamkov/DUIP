using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// User or program input to a surface for a time interval.
    /// </summary>
    public abstract class Input
    {
        /// <summary>
        /// Gets the length in time of the input interval.
        /// </summary>
        public abstract double Time { get; }
    }
}