using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A reference to an indexable collection of unnamed data.
    /// </summary>
    public abstract class Context : Content
    {
        /// <summary>
        /// Gets the root actor that can add and modify owned items to the context.
        /// </summary>
        public abstract Query<Actor> Editor { get; }

        /// <summary>
        /// Gets the root actor that can view this context.
        /// </summary>
        public abstract Query<Actor> Viewer { get; }

        /// <summary>
        /// Tries adding a static datum to the context using the current network user.
        /// </summary>
        public abstract Query<StaticDatum> AddStatic(Content Content);

        /// <summary>
        /// Tries adding a variable datum to the context using the current network user. The owner of the variable is
        /// the root actor that is allowed to modify its value.
        /// </summary>
        public abstract Query<VariableDatum> AddVariable(Actor Owner, Content Type, Content Value);

        /// <summary>
        /// Tries adding a variable datum to the context using the current network user as the owner.
        /// </summary>
        public abstract Query<VariableDatum> AddVariable(Content Type, Content Value);
    }
}