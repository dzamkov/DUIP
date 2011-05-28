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

        /// <summary>
        /// Creates an item through which the given content can be transfered, or returns null if not possible.
        /// </summary>
        public static Transfer.Item Export(Content Content)
        {
            return new Transfer.StringItem("Test");
        }

        /// <summary>
        /// Imports content from a transferable item, or returns null if not possible.
        /// </summary>
        public static Disposable<Content> Import(Transfer.Item Item)
        {
            if (Item is Transfer.StringItem)
            {
                return new StaticContent<Data>(null, null);
            }
            else
            {
                return null;
            }
        }
    }
}