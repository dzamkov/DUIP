//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DUIP.Core
{
    /// <summary>
    /// A class or data that can be stored and represented in binary form.
    /// </summary>
    public interface Serializable
    {
        /// <summary>
        /// Serializes the object into the specified stream.
        /// </summary>
        /// <param name="Target">The stream to serialize to.</param>
        void Serialize(BinaryWriteStream Target);
    }

    /// <summary>
    /// Helper class for serialization.
    /// </summary>
    public static class Serialize
    {

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
        /// Deserializes an object. This is accomplished by looking for a public constructor
        /// with a single BinaryReadStream parameter and calling or by looking for a "Deserialize"
        /// method that takes a BinaryReadStream and returns an object.
        /// </summary>
        /// <param name="Stream">The stream to deserialize the object from.</param>
        /// <param name="Type">The type of the object.</param>
        /// <returns>The deserialized object.</returns>
        public static Serializable DeserializeShort(BinaryReadStream Stream, Type Type)
        {
            ConstructorInfo ci = Type.GetConstructor(new Type[] { typeof(BinaryReadStream) });
            if (ci != null && ci.IsPublic && !Type.IsAbstract)
            {
                return (Serializable)ci.Invoke(new object[] { Stream }); 
            }
            else
            {
                MethodInfo mi = Type.GetMethod("Deserialize", new Type[] { typeof(BinaryReadStream) });
                if (mi != null && mi.ReturnType == Type)
                {
                    return (Serializable)mi.Invoke(null, new object[] { Stream });
                }
                else
                {
                    throw new Exception("No deserializer");
                }
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
            TypeDirectory.GetIDForType(Object.GetType()).Serialize(Stream);
            SerializeShort(Object, Stream);
        }


        /// <summary>
        /// Deserializes a long object.
        /// </summary>
        /// <param name="Stream">The stream to deserialize from.</param>
        /// <returns>The deserialized object.</returns>
        public static Serializable DeserializeLong(BinaryReadStream Stream)
        {
            Type Type = TypeDirectory.GetTypeByID(new ID(Stream));
            return DeserializeShort(Stream, Type);
        }
    }
}
