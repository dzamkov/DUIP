using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Immutable, serializable data that can be stored across a network.
    /// </summary>
    public abstract class Content
    {

    }

    /// <summary>
    /// A reference to a datum on the current network.
    /// </summary>
    public abstract class Reference : Content
    {
        /// <summary>
        /// Gets the ID of the referenced datum.
        /// </summary>
        public abstract ID ID { get; }

        /// <summary>
        /// Gets the current value of the referenced content.
        /// </summary>
        public abstract Query<Content> Value { get; }

        /// <summary>
        /// Gets he most specific type the value is guaranteed to be of.
        /// </summary>
        public abstract Query<Content> Type { get; }
    }
}