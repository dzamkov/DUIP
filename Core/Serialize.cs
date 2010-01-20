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
    /// Helper class for serialization.
    /// </summary>
    public static class Serialize
    {
        private const BindingFlags _AutoSerializedFields =
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        private const BindingFlags _InvokableMethods =
            BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.NonPublic;

        /// <summary>
        /// Automatically serializes an object based on members found in reflection.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        /// <param name="Type">The type of the object to serialize.</param>
        /// <param name="Stream">The stream to serialize to.</param>
        private static void _AutoSerialize(object Object, Type Type, BinaryWriteStream Stream)
        {
            foreach (FieldInfo fi in Type.GetFields(_AutoSerializedFields))
            {
                object val = fi.GetValue(Object);
                Type ty = fi.FieldType;
                SerializeShort(val, ty, Stream);
            }
        }

        /// <summary>
        /// Deserializes an object that was previously serialized with AutoSerialize.
        /// </summary>
        /// <param name="Object">The object to deserialize to.</param>
        /// <param name="Type">The type of the object to deserialize.</param>
        /// <param name="Stream">The stream to deserialize from.</param>
        private static void _AutoDeserialize(object Object, Type Type, BinaryReadStream Stream)
        {
            foreach (FieldInfo fi in Type.GetFields(_AutoSerializedFields))
            {
                Type ty = fi.FieldType;
                fi.SetValue(Object, DeserializeShort(null, ty, Stream));
            }
        }

        /// <summary>
        /// Serializes a short object. This type of serialization assumes that the
        /// type of the object is known at the receiving end.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        /// <param name="Type">The type of the object, or the type of the base of the object that
        /// needs to be serialized.</param>
        /// <param name="Stream">The stream to serialize the object into.</param>
        public static void SerializeShort(object Object, Type Type, BinaryWriteStream Stream)
        {
            // Simple serialization
            if (Type.IsPrimitive)
            {
                if (Type == typeof(int))
                {
                    Stream.WriteInt((int)(Object));
                }
                if (Type == typeof(double))
                {
                    Stream.WriteDouble((double)(Object));
                }
                if (Type == typeof(char))
                {
                    Stream.WriteChar((char)(Object));
                }
                return;
            }
            if (Type == typeof(string))
            {
                Stream.WriteString((string)(Object));
                return;
            }

            // Complex serialization
            MethodInfo ser = Type.GetMethod("Serialize", _InvokableMethods, null, new Type[] { typeof(BinaryWriteStream) }, null);
            if (ser != null && !ser.IsStatic)
            {
                ser.Invoke(Object, new object[] { Stream });
            }
            else
            {
                _AutoSerialize(Object, Type, Stream);
                if (Type.BaseType != null)
                {
                    SerializeShort(Object, Type.BaseType, Stream);
                }
            }
        }

        /// <summary>
        /// Deserializes an object. This is accomplished by looking for a constructor
        /// with a single BinaryReadStream parameter and calling or by looking for a "Deserialize"
        /// method that takes a BinaryReadStream and returns an object.
        /// </summary>
        /// <param name="Object">The object to deserialize to or null to construct the new object
        /// in this method.</param>
        /// <param name="Stream">The stream to deserialize the object from.</param>
        /// <param name="Type">The type of the object.</param>
        /// <returns>The deserialized object.</returns>
        public static object DeserializeShort(object Object, Type Type, BinaryReadStream Stream)
        {
            // Simple deserialization
            if (Type.IsPrimitive)
            {
                if (Type == typeof(int))
                {
                    return Stream.ReadInt();
                }
                if (Type == typeof(double))
                {
                    return Stream.ReadDouble();
                }
                if (Type == typeof(char))
                {
                    return Stream.ReadChar();
                }
                return null;
            }
            if (Type == typeof(string))
            {
                return Stream.ReadString();
            }

            // Complex deserialization
            if (Object == null)
            {
                ConstructorInfo ci = Type.GetConstructor(_InvokableMethods, null, new Type[] { typeof(BinaryReadStream) }, null);
                if (ci != null && !Type.IsAbstract)
                {
                    return ci.Invoke(new object[] { Stream }); 
                }

                MethodInfo mi = Type.GetMethod("Deserialize", _InvokableMethods, null, new Type[] { typeof(BinaryReadStream) }, null);
                if (mi != null && mi.IsStatic && mi.ReturnType == Type)
                {
                    return mi.Invoke(null, new object[] { Stream });
                }

                // Use default constructor to create object then deserialize in place.
                ci = Type.GetConstructor(new Type[0]);
                if (ci != null && !Type.IsAbstract)
                {
                    return DeserializeShort(ci.Invoke(new object[0]), Type, Stream);
                }
            }
            else
            {
                // Deserialize method.
                MethodInfo mi = Type.GetMethod("Deserialize", _InvokableMethods, null, new Type[] { typeof(BinaryReadStream) }, null);
                if (mi != null && !mi.IsStatic)
                {
                    mi.Invoke(Object, new object[] { Stream });
                    return Object;
                }

                // Autodeserialize
                _AutoDeserialize(Object, Type, Stream);
                if (Type.BaseType != null)
                {
                    DeserializeShort(Object, Type.BaseType, Stream);
                }
                return Object;
            }

            throw new Exception("Nope, I cant deserialize this, too hard");
        }

        /// <summary>
        /// Serializes a long object. The objects type is stored along with the data and is
        /// not needed to be known at the receiving end.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        /// <param name="Stream">The stream to serialize to.</param>
        public static void SerializeLong(object Object, BinaryWriteStream Stream)
        {
            Type objtype = Object.GetType();
            TypeDirectory.GetIDForType(objtype).Serialize(Stream);
            SerializeShort(Object, objtype, Stream);
        }


        /// <summary>
        /// Deserializes a long object.
        /// </summary>
        /// <param name="Stream">The stream to deserialize from.</param>
        /// <returns>The deserialized object.</returns>
        public static object DeserializeLong(BinaryReadStream Stream)
        {
            Type Type = TypeDirectory.GetTypeByID(new ID(Stream));
            return DeserializeShort(null, Type, Stream);
        }
    }
}
