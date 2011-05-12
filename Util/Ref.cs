using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Wraps an object of a certain type with a reference.
    /// </summary>
    public class Ref<T>
    {
        public Ref()
        {

        }

        public Ref(T Value)
        {
            this.Value = Value;
        }

        public static implicit operator T(Ref<T> Reference)
        {
            return Reference.Value;
        }

        public static implicit operator Ref<T>(T Value)
        {
            return new Ref<T>(Value);
        }

        /// <summary>
        /// The value of this reference.
        /// </summary>
        public T Value;
    }
}