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
            string text = Clipboard.GetText();
            if (text != null)
            {
                return new StringItem(text);
            }

            return null;
        }

        /// <summary>
        /// Gets an item from a data object.
        /// </summary>
        public static Item FromDataObject(IDataObject Object)
        {
            string text = Object.GetData("Text") as string;
            if (text != null)
            {
                return new StringItem(text);
            }

            return null;
        }

        /// <summary>
        /// Sets the clipboard to this item.
        /// </summary>
        public abstract void SetClipboard();
    }
}