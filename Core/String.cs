using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type for unicode strings.
    /// </summary>
    public class StringType : Type<string>
    {
        private StringType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly StringType Singleton = new StringType();

        public override bool Equal(string A, string B)
        {
            return A == B;
        }

        public override UI.Block CreateBlock(UI.Theme Theme, string Instance)
        {
            return UI.Block.Text(Instance, Theme.GetFont(UI.FontPurpose.General), Theme.FlowStyle)
                .WithPad(Theme.TextPadding);
        }
    }
}