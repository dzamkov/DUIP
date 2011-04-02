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
        /// <summary>
        /// Tries getting a named property of the content.
        /// </summary>
        public virtual Query<Content> Get(string Property)
        {
            return new StaticQuery<Content>(null);
        }

        /// <summary>
        /// Tries calling this content as a function with the given parameter.
        /// </summary>
        public virtual Query<Content> Call(Content Parameter)
        {
            return new StaticQuery<Content>(null);
        }
    }
}