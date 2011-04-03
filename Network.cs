﻿using System;
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
        /// Looks for the datum with the specified index in the network.
        /// </summary>
        public abstract Query<Datum> this[ID Index] { get; }

        /// <summary>
        /// Gets the root context for the network.
        /// </summary>
        public abstract Query<Context> Root { get; }
    }
}