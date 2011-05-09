using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An object with a requirement to be disposed when no longer needed. If this structure is given as a result of
    /// a method, it is assumed that the caller will be responsible for the management of the object.
    /// </summary>
    public struct Disposable<T> : IDisposable
        where T : class
    {
        public Disposable(T Object)
        {
            this.Object = Object;
        }

        public void Dispose()
        {
            IDisposable dis = Object as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
        }

        public static implicit operator T(Disposable<T> Disposable)
        {
            return Disposable.Object;
        }

        public static implicit operator Disposable<T>(T Object)
        {
            return new Disposable<T>(Object);
        }

        /// <summary>
        /// Modifies the given parameter to have this object, disposing when needed.
        /// </summary>
        public void Mutate(ref Disposable<T> Variable)
        {
            if (this.Object != Variable.Object)
            {
                Variable.Dispose();
            }
            Variable.Object = this.Object;
        }

        /// <summary>
        /// The object to be used.
        /// </summary>
        public T Object;
    }
}