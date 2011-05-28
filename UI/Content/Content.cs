using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
        /// Creates content for the given data object, or returns null if not possible.
        /// </summary>
        public static Disposable<Content> Import(IDataObject Object)
        {
            string[] formats = Object.GetFormats();
            object filename = Object.GetData("FileName");
            if (filename != null)
            {
                return new StaticContent<Data>(null, null);
            }

            return null;
        }

        /// <summary>
        /// Creates a data object that represents the given content.
        /// </summary>
        public static DataObject Export(Content Content)
        {
            DataObject data = new DataObject();
            return data;
        }
    }
}