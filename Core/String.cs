using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type for unicode strings.
    /// </summary>
    public class StringType : Type
    {
        private StringType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly StringType Singleton = new StringType();

        public override bool Equal(object A, object B)
        {
            return A as string == B as string;
        }

        public override UI.Block CreateBlock(object Instance, UI.Theme Theme)
        {
            return Theme.TextBlock(Instance as string);
        }
    }
}