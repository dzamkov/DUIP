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

    }

    /// <summary>
    /// A datum that defines a context.
    /// </summary>
    public abstract class ContextDatum : Datum
    {

    }

    /// <summary>
    /// A datum that stores and represents content.
    /// </summary>
    public abstract class ContentDatum : Datum
    {
        /// <summary>
        /// Gets the most specific type for the allowable content in this datum.
        /// </summary>
        public abstract Content<Type> Type { get; }

        /// <summary>
        /// Gets the current content in the datum.
        /// </summary>
        public abstract Content Content { get; }
    }

    /// <summary>
    /// An immutable datum that stores content.
    /// </summary>
    public abstract class StaticDatum : ContentDatum
    {

    }

    /// <summary>
    /// A mutable datum that stores content which can be modified by the owner actor.
    /// </summary>
    public abstract class VariableDatum : ContentDatum
    {
        /// <summary>
        /// Gets the root actor which can modify the value of this variable datum.
        /// </summary>
        public abstract Content<Actor> Owner { get; }

        /// <summary>
        /// Sets the value of the datum directly using the current user for the network this datum is for. Returns true
        /// on success or false on failure. Note that the value must be of the type for the datum.
        /// </summary>
        public abstract Query<bool> Modify(Content Value);

        /// <summary>
        /// Transfers ownership of the datum to another actor.
        /// </summary>
        public abstract Query<bool> Transfer(Actor Actor);
    }
}