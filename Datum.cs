using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Data stored with an index on a network.
    /// </summary>
    public abstract class Datum
    {
        /// <summary>
        /// Gets the ID of the datum.
        /// </summary>
        public abstract ID ID { get; }

        /// <summary>
        /// Gets the current content of the datum.
        /// </summary>
        public abstract Query<Content> Content { get; }
    }

    /// <summary>
    /// An immutable datum that stores content.
    /// </summary>
    public abstract class StaticDatum : Datum
    {

    }

    /// <summary>
    /// A mutable datum that stores content which can be modified by the owner user.
    /// </summary>
    public abstract class VariableDatum : Datum
    {
        /// <summary>
        /// Gets the user which can modify the value of this variable datum.
        /// </summary>
        public abstract Query<User> Owner { get; }

        /// <summary>
        /// Gets the type the datum is constrainted to.
        /// </summary>
        public abstract Query<Content> Type { get; }

        /// <summary>
        /// Sets the value of the datum directly using the current user for the network this datum is for. Returns true
        /// on success or false on failure.
        /// </summary>
        public abstract Query<bool> Modify(Content Value);
    }
}