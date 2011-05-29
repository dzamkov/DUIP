using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DUIP.UI.Transfer
{
    /// <summary>
    /// An item that can be transfered between applications through drag and drop operations or the clipboard.
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// Gets the item currently on the clipboard.
        /// </summary>
        public static Item FromClipboard()
        {
            if (Clipboard.ContainsText())
            {
                return new StringItem(Clipboard.GetText());
            }

            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                if (files.Count == 1)
                {
                    return new StandardFileItem(files[0]);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an item from a data object.
        /// </summary>
        public static Item FromDataObject(IDataObject Object)
        {
            string[] formats = Object.GetFormats();

            string text = Object.GetData("Text") as string;
            if (text != null)
            {
                return new StringItem(text);
            }

            string[] file = (Object.GetData("FileNameW") as string[]) ?? (Object.GetData("FileName") as string[]);
            if (file != null && file.Length == 1)
            {
                return new StandardFileItem(file[0]);
            }

            return null;
        }

        /// <summary>
        /// Sets the clipboard to this item.
        /// </summary>
        public abstract void SetClipboard();
    }
}