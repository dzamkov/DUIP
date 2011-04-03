using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An indexable collection of unnamed data.
    /// </summary>
    public abstract class Context : Content
    {
        /// <summary>
        /// Gets the root user that can add and modify owned items to the context.
        /// </summary>
        public abstract Query<User> Editor { get; }

        /// <summary>
        /// Gets the root user that can execute operations within the context.
        /// </summary>
        public abstract Query<User> Executor { get; }

        /// <summary>
        /// Gets the root user that can view this context.
        /// </summary>
        public abstract Query<User> Viewer { get; }

        /// <summary>
        /// Tries adding a static datum to the context using the current network user.
        /// </summary>
        public abstract Query<StaticDatum> AddStatic(Content Content);

        /// <summary>
        /// Tries adding a variable datum to the context using the current network user. The owner of the variable is
        /// the root user that is allowed to modify its value.
        /// </summary>
        public abstract Query<VariableDatum> AddVariable(User Owner, Content Type, Content Value);

        /// <summary>
        /// Tries adding a variable datum to the context using the current network user with the owner being the current network user.
        /// </summary>
        public abstract Query<VariableDatum> AddVariable(Content Type, Content Value);
    }
}