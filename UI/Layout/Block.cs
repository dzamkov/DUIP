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
        public abstract Control CreateControl(Point Size, ControlEnvironment Environment);
    }
}