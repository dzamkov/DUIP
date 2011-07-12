using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A function that removes a callback added with a Register method. It is possible for remove handlers
    /// to be null if no process is needed to remove a callback (the callback will never be called).
    /// </summary>
    public delegate void RemoveHandler();
}