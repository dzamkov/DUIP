using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type whose two instances are true and false.
    /// </summary>
    public class BoolType : Type<bool>
    {
        private BoolType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly BoolType Singleton = new BoolType();
    }
}