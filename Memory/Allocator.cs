using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A representation of storage space that allows the allocation and deallocation of fixed-size mutable memory.
    /// </summary>
    /// <typeparam name="T">A pointer to allocated memory in the allocator.</typeparam>
    public abstract class Allocator<T>
    {
        /// <summary>
        /// Allocates memory with the given size. The resulting memory will be null if there is no
        /// space for allocation available.
        /// </summary>
        public abstract Memory Allocate(long Size, out T Pointer);

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

        /// <summary>
        /// Gets the memory at the given pointer. The size of the returned data may be larger than the size of the allocated
        /// data, in which case, the extra bytes are safe to use.
        /// </summary>
        public abstract Memory Lookup(T Pointer);

        /// <summary>
        /// Deallocates the memory with the given pointer. After this is called, the corresponding memory
        /// may no longer be used.
        /// </summary>
        public abstract void Deallocate(T Pointer);
    }
}