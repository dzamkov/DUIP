//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUIP.Core
{
    /// <summary>
    /// A class or data that can be stored and represented in binary form. Note that
    /// the object should alway
    /// </summary>
    public interface Serializable
    {
        /// <summary>
        /// Serializes the object into the specified stream.
        /// </summary>
        /// <param name="Target">The stream to serialize to.</param>
        void Serialize(BinaryWriteStream Target);

        /// <summary>
        /// Gets the unique type identifier of this object.
        /// </summary>
        ID TypeID { get; }
    }

    /// <summary>
    /// Helper class for serialization.
    /// </summary>
    public static class Serialize
    {
        static Serialize()
        {
            Deserializers = new Dictionary<ID, DeserializeHandler>();
        }

        /// <summary>
        /// Registers a deserializer for an id.
        /// </summary>
        /// <param name="Identifier">The type ID of objects to be deserialized.</param>
        /// <param name="Deserializer">The deserializer used to deserialize those objects.</param>
        public static void RegisterDeserializer(ID Identifier, DeserializeHandler Deserializer)
        {
            if (Deserializers.ContainsKey(Identifier))
            {
                throw new Exception("Type ID already exists");
            }
            else
            {
                Deserializers.Add(Identifier, Deserializer);
            }
        }

        /// <summary>
        /// Creates a type identifier for a type.
        /// </summary>
        /// <param name="TypeName">The name of the type.</param>
        /// <returns>A possible ID for the type.</returns>
        public static ID CreateTypeID(string TypeName)
        {
            return ID.Hash("TYPE: " + TypeName);
        }

        /// <summary>
        /// Serializes a short object. This type of serialization assumes that the
        /// type of the object is known at the receiving end.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        /// <param name="Stream">The stream to serialize the object into.</param>
        public static void SerializeShort(Serializable Object, BinaryWriteStream Stream)
        {
            Object.Serialize(Stream);
        }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <param name="Stream">The stream to deserialize the object from.</param>
        /// <param name="Type">The type of the object.</param>
        /// <returns>The deserialized object.</returns>
        public static Serializable DeserializeShort(BinaryReadStream Stream, ID Type)
        {
            DeserializeHandler dh;
            if (Deserializers.TryGetValue(Type, out dh))
            {
                return dh(Stream);
            }
            else
            {
                throw new Exception("No deserializer");
            }
        }

        /// <summary>
        /// Serializes a long object. The objects type is stored along with the data and is
        /// not needed to be known at the receiving end.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        /// <param name="Stream">The stream to serialize to.</param>
        public static void SerializeLong(Serializable Object, BinaryWriteStream Stream)
        {
            ((Serializable)Object.TypeID).Serialize(Stream);
            SerializeShort(Object, Stream);
        }


        /// <summary>
        /// Deserializes a long object.
        /// </summary>
        /// <param name="Stream">The stream to deserialize from.</param>
        /// <returns>The deserialized object.</returns>
        public static Serializable DeserializeLong(BinaryReadStream Stream)
        {
            ID Type = (ID)(ID.Deserialize(Stream));
            return DeserializeShort(Stream, Type);
        }

        /// <summary>
        /// Deserialize handlers for various id's.
        /// </summary>
        private static Dictionary<ID, DeserializeHandler> Deserializers;
    }

    /// <summary>
    /// Handler for deserializing from a byte stream.
    /// </summary>
    /// <param name="Stream">The binary stream source to deserialize from.</param>
    /// <returns>The original serializable object.</returns>
    public delegate Serializable DeserializeHandler(BinaryReadStream Stream);
}
