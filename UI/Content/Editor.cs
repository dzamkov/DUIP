using System;
using System.Collections.Generic;
using System.Linq;

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
            TextBlock textpad = new TextBlock(new TextStyle
            {
                CellSize = new Point(0.05, 0.05),
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
                DefaultFontStyle = new TextFontStyle
                {
                    Font = Theme.MonospaceTypeface.Object.GetFont(0.05, Color.Black),
                    SelectedFont = Theme.MonospaceTypeface.Object.GetFont(0.05, Color.White)
                },
                DefaultBackStyle = new TextBackStyle
                {
                    BackColor = Color.Transparent,
                    SelectedBackColor = Color.RGB(0.0, 0.0, 1.0)
                },
                IndentSize = 3
            });
            textpad.Insert(textpad.End, TextSection.Create(
@"Look, it's a list:
    * With
    * Three
    * Items"));
            return textpad.WithMargin(0.1).WithBackground(Theme.NodeBackground).WithBorder(Theme.NodeBorder);
        }
    }
}