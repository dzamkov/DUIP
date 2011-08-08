using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// Content that displays a text block to allow the user to create objects based on written code.
    /// </summary>
    public class EditorContent : Content
    {
        public EditorContent()
        {

        }

        public override Disposable<Block> CreateBlock(Theme Theme)
        {
            SystemTypeface typeface = SystemTypeface.Create("Courier New", false, false);
            TextBlock textblock =
                new TextBlock("Look, it's a list:\n\t* With\n\t* Three\n\t* Items",
                    new TextBlockStyle(new Point(0.04, 0.05), Alignment.Center, Alignment.Center, 4, 0.5, new Border(0.003, Color.Black)));
            return textblock.WithMargin(0.1).WithBackground(Theme.NodeBackground).WithBorder(Theme.NodeBorder);
        }
    }
}