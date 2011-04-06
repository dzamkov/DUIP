using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type of data that can be stored on the network. It may be assumed that types with the same instance
    /// type will not have their instances mixed.
    /// </summary>
    /// <remarks>Types can be viewed as a range of possible values and a 
    /// way to convert them into a data representation.</remarks>
    /// <typeparam name="TInstance">An instance of this type.</typeparam>
    public abstract class Type<TInstance> : Type
    {
        /// <summary>
        /// Serializes an instance of this type to an output stream.
        /// </summary>
        public abstract void Serialize(TInstance Instance, OutByteStream Stream);

        /// <summary>
        /// Deserializes an instance of this type from a stream, or returns null if not possible.
        /// </summary>
        public abstract Query<TInstance> Deserialize(InByteStream Stream);
    }

    /// <summary>
    /// A generalization of type with no specific instance type.
    /// </summary>
    public abstract class Type
    {

    }
}