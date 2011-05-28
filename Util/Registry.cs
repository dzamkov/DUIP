using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A collection of weakly-referenced cached objects of a certain type.
    /// </summary>
    public class Registry<T> : IEnumerable<T>
        where T : class
    {
        public Registry()
        {
            this._Objects = new LinkedList<WeakReference>();
        }

        /// <summary>
        /// Adds the given object to the registry.
        /// </summary>
        public void Register(T Object)
        {
            this._Objects.AddFirst(new WeakReference(Object));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// An enumerator for a registry.
        /// </summary>
        public class Enumerator : IEnumerator<T>
        {
            public Enumerator(Registry<T> Registry)
            {
                this._Objects = Registry._Objects;   
            }

            public T Current
            {
                get 
                {
                    return this._Node.Value.Target as T;
                }
            }

            public void Dispose()
            {
                // Bring the current node to the front, it will probably be accessed again soon
                if (this._Node != null)
                {
                    WeakReference val = this._Node.Value;
                    this._Objects.Remove(this._Node);
                    this._Objects.AddFirst(val);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get 
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                if (this._Node == null)
                {
                    this._Node = this._Objects.First;
                }
                else
                {
                    this._Node = this._Node.Next;
                }
                while (true)
                {
                    if (this._Node == null)
                    {
                        return false;
                    }
                    if (this._Node.Value.IsAlive)
                    {
                        return true;
                    }
                    else
                    {
                        LinkedListNode<WeakReference> next = this._Node.Next;
                        this._Objects.Remove(this._Node);
                        this._Node = next;
                    }
                }
            }

            public void Reset()
            {
                this._Node = null;
            }

            private LinkedList<WeakReference> _Objects;
            private LinkedListNode<WeakReference> _Node;
        }

        private LinkedList<WeakReference> _Objects;
    }
}