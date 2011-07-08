using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Content that displays a text pad to allow the user to create objects based on written code.
    /// </summary>
    public class EditorContent : Content
    {
        public EditorContent()
        {

        }

        public override Disposable<Block> CreateBlock(Theme Theme)
        {
            Border border; Color background;
            Theme.GetNodeStyle(out border, out background);
            TextPad textpad = new TextPad(new TextPadStyle
            {
                CellSize = new Point(0.1, 0.1),
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
                IndentSize = 3
            });
            textpad.InsertText(textpad.Start, "Test text", new TextPad.TextStyle
            {
                Font = Theme.GetFont(FontPurpose.General),
                SelectedFont = Theme.GetFont(FontPurpose.General),
                BackColor = Color.Transparent,
                SelectedBackColor = Color.RGB(1.0, 1.0, 0.5),
            });
            return textpad.WithBackground(background).WithBorder(border);
        }
    }
}