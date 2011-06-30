using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An object that will eventually be reused, modified, or otherwise made unusable. When given as an argument, the object
    /// should not be referenced after the function or method returns.
    /// </summary>
    public struct Temporary<T>
    {
        public Temporary(T Object)
        {
            this.Object = Object;
        }

        public static implicit operator T(Temporary<T> Temporary)
        {
            return Temporary.Object;
        }

        public static implicit operator Temporary<T>(T Object)
        {
            return new Temporary<T>(Object);
        }

        /// <summary>
        /// The object to be used.
        /// </summary>
        public T Object;
    }
}