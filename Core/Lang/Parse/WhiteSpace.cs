using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Lang.Parse
{
    /// <summary>
    /// A parser for some amount of whitespace.
    /// </summary>
    public class WhiteSpaceParser : Parser<Void>
    {
        private WhiteSpaceParser()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly WhiteSpaceParser Singleton = new WhiteSpaceParser();

        public override bool Accept(ref Text Text, ref Void Object)
        {
            bool haswhitespace = false;
            while (Text.Length > 0)
            {
                char c = Text[0];
                if (c == ' ' || c == '\n' || c == '\r' || c == '\t')
                {
                    Text = Text.Sub(1);
                    haswhitespace = true;
                }
                else
                {
                    break;
                }
            }
            return haswhitespace;
        }
    }
}