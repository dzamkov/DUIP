﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DUIP
{
    /// <summary>
    /// Contains functions related to reflection.
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Searches for all attributes of a certain type applied to type definitions in the current assembly.
        /// </summary>
        public static IEnumerable<KeyValuePair<System.Type, T>> SearchAttributes<T>()
            where T : Attribute
        {
            System.Type attrtype = typeof(T);
            foreach (System.Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (object obj in type.GetCustomAttributes(attrtype, true))
                {
                    yield return new KeyValuePair<System.Type, T>(type, (T)obj);
                }
            }
        }

        /// <summary>
        /// Casts a static method or member to a certain type or returns nothing if not possible.
        /// </summary>
        public static Maybe<T> Cast<T>(System.Type Type, string Name)
        {
            const BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.DeclaredOnly |
                BindingFlags.Public;

            // Try as method
            foreach (MethodInfo method in Type.GetMethods(flags))
            {
                if(method.Name == Name)
                {
                    Delegate del = Delegate.CreateDelegate(typeof(T), method, false);
                    if (del != null)
                    {
                        return Maybe<T>.Just((T)(object)del);
                    }
                }
            }
                
            // Try as field
            FieldInfo field = Type.GetField(Name, flags);
            if (field != null)
            {
                object val = field.GetValue(null);
                if (val is T)
                {
                    return Maybe<T>.Just((T)val);
                }
                else
                {
                    return Maybe<T>.Nothing;
                }
            }

            // Default
            return Maybe<T>.Nothing;
        }
    }
}