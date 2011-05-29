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
        public abstract Disposable<Visual> CreateVisual(Theme Theme);

        /// <summary>
        /// Creates an item through which the given content can be transfered, or returns null if not possible.
        /// </summary>
        public static Transfer.Item Export(Content Content)
        {
            StaticContent<string> sc = Content as StaticContent<string>;
            if (sc != null)
            {
                return new Transfer.StringItem(sc.Value);
            }

            return null;
        }

        /// <summary>
        /// Imports content from a transferable item, or returns null if not possible.
        /// </summary>
        public static Disposable<Content> Import(Transfer.Item Item)
        {
            Transfer.StringItem si = Item as Transfer.StringItem;
            if (si != null)
            {
                return new StaticContent<string>(si.String, Type.String);
            }

            Transfer.FileItem fi = Item as Transfer.FileItem;
            if (fi != null)
            {
                return new StaticContent<File>(fi.GetFile(), Type.File);
            }

            return null;
        }
    }
}