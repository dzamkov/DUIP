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

    /// <summary>
    /// An allocator that stores memory as files in a directory. Memory in this kind of allocator will persist between
    /// instances and processes.
    /// </summary>
    public class DirectoryAllocator : Allocator<int>
    {
        public DirectoryAllocator(Path Directory, Random Random)
        {
            this._Random = Random;
            this._Directory = Directory;
            this._Directory.MakeDirectory();
        }

        public DirectoryAllocator(Path Directory)
            : this(Directory, new Random())
        {

        }

        /// <summary>
        /// Gets the directory used by the allocator.
        /// </summary>
        public Path Directory
        {
            get
            {
                return this._Directory;
            }
        }

        /// <summary>
        /// Gets the random source used for generating file indices.
        /// </summary>
        public Random Random
        {
            get
            {
                return this._Random;
            }
        }

        public override Memory Allocate(long Size, out int Pointer)
        {
            Path fpath;
            do
            {
                Pointer = this._Random.Next(int.MinValue, int.MaxValue);
                fpath = this._GetFile(Pointer);
            }
            while (fpath.FileExists);
            return fpath.Create(Size);
        }

        public override Memory Lookup(int Pointer)
        {
            return this._GetFile(Pointer).Open();
        }

        public override void Deallocate(int Pointer)
        {
            this._GetFile(Pointer).Delete();
        }

        /// <summary>
        /// Gets the name of the file for the given index.
        /// </summary>
        private string _GetName(int Index)
        {
            return Index.ToString("X");
        }

        /// <summary>
        /// Gets the path for the file with the given index.
        /// </summary>
        private Path _GetFile(int Index)
        {
            return this._Directory[this._GetName(Index)];
        }

        private Random _Random;
        private Path _Directory;
    }
}