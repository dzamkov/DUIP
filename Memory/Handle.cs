using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Memory
{
    /// <summary>
    /// A high-level interface to memory.
    /// </summary>
    /// <remarks>There are certain terms used in the context of handles. "Scheme" refers to information that can
    /// not be stored in memory and must be supplied to the handle. A "Plan" is information needed to initialize the
    /// data of a handle for the first time.</remarks>
    public interface IHandle
    {
        /// <summary>
        /// Gets the memory source for this handle.
        /// </summary>
        Memory Source { get; }
    }
}