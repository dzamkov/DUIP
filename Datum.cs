using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A reference to mutable information stored in the network.
    /// </summary>
    public abstract class Datum
    {
        /// <summary>
        /// Gets the global ID for this datum.
        /// </summary>
        public abstract ID ID { get; }

        /// <summary>
        /// Gets the context this datum is in. The root context and root user are the only data without a parent context.
        /// </summary>
        public abstract Query<Context> GetContext();
    }
}