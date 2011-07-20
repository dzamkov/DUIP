using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using DUIP.Memory;

namespace DUIP.UI.Transfer
{
    /// <summary>
    /// An item representation of a file (or folder).
    /// </summary>
    public abstract class FileItem : Item
    {
        /// <summary>
        /// Gets the file representation of the item. This may have additional effects depending on 
        /// method used to transfer the item.
        /// </summary>
        public abstract File GetFile();
    }

    /// <summary>
    /// A file item that references a "physical" file on the filesystem.
    /// </summary>
    public sealed class StandardFileItem : FileItem
    {
        public StandardFileItem(Path Path)
        {
            this._Path = Path;
        }

        public override void SetClipboard()
        {
            throw new NotImplementedException();
        }

        public override File GetFile()
        {
            return this._Path.GetFile();
        }

        private Path _Path;
    }
}