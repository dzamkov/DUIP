using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A mutable description of visual contents within a rectangular area.
    /// </summary>
    public abstract class Block
    {
        /// <summary>
        /// Creates a control for this block using the given environment. The control will be updated
        /// to reflect changes to this block.
        /// </summary>
        public abstract Control CreateControl(ControlEnvironment Environment);
    }
}