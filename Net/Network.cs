using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interface to a peer in a distributed store.
    /// </summary>
    public abstract class Network<T> : Store<T>
    {
        /// <summary>
        /// Stores a piece of data in the network.
        /// </summary>
        public abstract Query<T> Add(Data Data);
    }
}