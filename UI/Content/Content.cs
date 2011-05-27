using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// An object the user can view or interact with.
    /// </summary>
    public abstract class Content
    {
        /// <summary>
        /// Creates a visual representation of this content to be used within a node.
        /// </summary>
        public abstract Disposable<Visual> CreateVisual();
    }
}