using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A description of visual contents within a rectangular area.
    /// </summary>
    public abstract class Block
    {
        /// <summary>
        /// Creates a dynamic control (instance) of this block with the given parameters.
        /// </summary>
        public abstract Disposable<Control> CreateControl(ControlEnvironment Environment);

        /// <summary>
        /// Applies the given border to this block.
        /// </summary>
        public BorderBlock WithBorder(Border Border)
        {
            return new BorderBlock(Border, this);
        }

        /// <summary>
        /// Applies a background with the given color to this block.
        /// </summary>
        public BackgroundBlock WithBackground(Color Color)
        {
            return new BackgroundBlock(Color, this);
        }

        /// <summary>
        /// Gets a space block.
        /// </summary>
        public static SpaceBlock Space
        {
            get
            {
                return SpaceBlock.Singleton;
            }
        }
    }
}