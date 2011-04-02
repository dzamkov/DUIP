using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interface to an external network.
    /// </summary>
    public abstract class Network
    {
        /// <summary>
        /// Gets the current user used for querying the network.
        /// </summary>
        public abstract User User { get; }

        /// <summary>
        /// Gets the proof used to authenticate as the current user.
        /// </summary>
        public abstract Proof Proof { get; }

        /// <summary>
        /// Tries getting or setting the content with the given global network index.
        /// </summary>
        public abstract Query<Content> this[ID Index] { get; set; }
    }
}