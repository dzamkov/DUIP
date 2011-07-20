using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Memory
{
    /// <summary>
    /// A representation of storage space that allows the allocation and deallocation of fixed-size mutable memory.
    /// </summary>
    /// <typeparam name="T">A pointer to allocated memory in the allocator.</typeparam>
    public abstract class Allocator<T>
    {
        /// <summary>
        /// Tries allocating memory with the given size. Returns null if not possible.
        /// </summary>
        public abstract Memory Allocate(long Size, out T Pointer);

        /// <summary>
        /// Deallocates the memory with the given pointer. The size given must be the size of the allocated memory.
        /// </summary>
        public abstract void Deallocate(long Size, T Pointer);

        /// <summary>
        /// Gets the memory for the given pointer. The size of the returned memory must be at least the size of the allocated memory. If
        /// the returned memory is larger than the allocated memory, only the intial bytes of the size of the allocated memory may be used.
        /// </summary>
        public abstract Memory Lookup(T Pointer);

        /// <summary>
        /// Gets the maximum size for which an allocation can succeed.
        /// </summary>
        public virtual long MaxSize
        {
            get
            {
                return long.MaxValue;
            }
        }
    }
}