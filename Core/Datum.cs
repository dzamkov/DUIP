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
        /// Gets the root actor that can view this datum. The universal actor can be used to indicate that this datum
        /// is unsecured.
        /// </summary>
        public abstract Query<Actor> Viewer { get; }
    }

    /// <summary>
    /// A datum that stores raw data without giving additional information of how to interpret it.
    /// </summary>
    public abstract class RawDatum : Datum
    {
        /// <summary>
        /// Gets a stream for the raw data in the datum.
        /// </summary>
        public abstract InByteStream Read { get; }
    }

    /// <summary>
    /// A datum that stores and represents typed content.
    /// </summary>
    public abstract class ContentDatum : Datum
    {

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

    }
}