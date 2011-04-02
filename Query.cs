using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An executable command that outputs a result of a certain type when complete.
    /// </summary>
    public abstract class Query<T>
    {
        /// <summary>
        /// Synchronously executes the query and gets its result.
        /// </summary>
        public abstract T Execute();
    }
}