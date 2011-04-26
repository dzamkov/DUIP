using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Lang.Parse
{
    /// <summary>
    /// A parser for any valid variable name or keyword.
    /// </summary>
    public class WordParser : Parser<Text>
    {
        private WordParser()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly WordParser Singleton = new WordParser();

        public override bool Accept(ref Text Text, ref Text Object)
        {
            int n = 0;
            while(n < Text.Length)
            {
                char c = Text[n];
                if (IsWordChar(c) || (IsDigitChar(c) && n > 0))
                {
                    n++;
                }
                else
                {
                    break;
                }
            }

            if (n > 0)
            {
                Object = Text.Sub(0, n);
                Text = Text.Sub(n);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets if the given character is valid in a word and is not a digit.
        /// </summary>
        public static bool IsWordChar(char Character)
        {
            int ascii = (int)Character;
            if (ascii >= 95 && ascii <= 122) return true;  // _ ` Lowercase
            if (ascii >= 65 && ascii <= 90) return true; // Capitals
            return false;
        }

        /// <summary>
        /// Gets if the given character is a digit.
        /// </summary>
        public static bool IsDigitChar(char Character)
        {
            int ascii = (int)Character;
            return (ascii >= 48 && ascii <= 57);
        }
    }
}