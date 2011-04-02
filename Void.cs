using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A dataless object with only one value.
    /// </summary>
    public struct Void
    {
        /// <summary>
        /// Gets the only value of this object.
        /// </summary>
        public static Void Value = new Void();
    }
}