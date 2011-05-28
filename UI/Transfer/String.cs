using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DUIP.UI.Transfer
{
    /// <summary>
    /// An item representation of a string.
    /// </summary>
    public class StringItem : Item
    {
        public StringItem(string String)
        {
            this._String = String;
        }

        /// <summary>
        /// Gets the string for this item.
        /// </summary>
        public string String
        {
            get
            {
                return this._String;
            }
        }

        public override void SetClipboard()
        {
            Clipboard.Clear();
            Clipboard.SetText(this._String);
        }

        private string _String;
    }
}