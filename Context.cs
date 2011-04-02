using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An indexable collection of unnamed data.
    /// </summary>
    public abstract class Context : Datum
    {
        /// <summary>
        /// Gets the owner of the context. The owner may change the setting of the context and any modify any datum within the context.
        /// </summary>
        public abstract Query<User> GetOwner();

        /// <summary>
        /// Gets the editor of the context. The editor may modify or add data within the context if permissible and not change context-wide settings.
        /// </summary>
        public abstract Query<User> GetEditor();
    }
}