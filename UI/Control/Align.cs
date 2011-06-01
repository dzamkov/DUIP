using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A possible relation of a two-dimensional item within a container on a single axis.
    /// </summary>
    public enum Alignment
    {
        Left = 0,
        Up = 0,
        Center = 1,
        Right = 2,
        Down = 2
    }

    /// <summary>
    /// Contains functions related to the alignment of items within containers.
    /// </summary>
    public static class Align
    {
        /// <summary>
        /// Gets the offset of an item from the top/left edge of its container based on the given alignment mode.
        /// </summary>
        public static double AxisOffset(Alignment Alignment, double ContainerSize, double ItemSize)
        {
            switch (Alignment)
            {
                case Alignment.Up: return 0.0;
                case Alignment.Center: return (ContainerSize - ItemSize) * 0.5;
                default: return ContainerSize - ItemSize;
            }
        }
    }
}