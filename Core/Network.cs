using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interface to an external network.
    /// </summary>
    /// <typeparam name="TRef">Reference to a datum in the network.</typeparam>
    public abstract class Network<TRef> : Network
    {
        /// <summary>
        /// Looks for the datum with the specified index in the network.
        /// </summary>
        public abstract Query<Datum> this[TRef Index] { get; }

        /// <summary>
        /// Serializes a network reference to a stream.
        /// </summary>
        public abstract void Serialize(TRef Ref, OutStream Stream);

        /// <summary>
        /// Deserializes a network reference from a stream.
        /// </summary>
        public abstract TRef Deserialize(InStream Stream);

        public sealed override void Serialize(Reference Reference, OutStream Stream)
        {
            Reference<TRef> tref = Reference as Reference<TRef>;
            this.Serialize(tref.Index, Stream);
        }

        public override Reference Deserialize(Reference Reference, InStream Stream)
        {
            return new Reference<TRef>(this.Deserialize(Stream));
        }
    }

    /// <summary>
    /// A generalized network with no specified reference type.
    /// </summary>
    public abstract class Network : Context
    {
        /// <summary>
        /// Gets the credential used in the network.
        /// </summary>
        public abstract Credential Credential { get; }

        /// <summary>
        /// Serializes a reference for this network to the given output stream.
        /// </summary>
        public abstract void Serialize(Reference Reference, OutStream Stream);

        /// <summary>
        /// Deserializes a reference for this network from a input stream.
        /// </summary>
        public abstract Reference Deserialize(Reference Reference, InStream Stream);
    }
}