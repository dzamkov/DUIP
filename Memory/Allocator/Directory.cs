using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Memory
{
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
            : this(Directory, Random.Default)
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

        /// <summary>
        /// Tries allocating memory at the given pointer. Returns null if not possible.
        /// </summary>
        public Memory Allocate(long Size, int Pointer)
        {
            Path fpath = this._GetFile(Pointer);
            if (fpath.Exists)
            {
                return null;
            }
            else
            {
                return fpath.Create(Size);
            }
        }

        public override Memory Allocate(long Size, out int Pointer)
        {
            Path fpath;
            do
            {
                Pointer = this._Random.Integer();
                fpath = this._GetFile(Pointer);
            }
            while (fpath.FileExists);
            return fpath.Create(Size);
        }

        public override void Deallocate(long Size, int Pointer)
        {
            this._GetFile(Pointer).Delete();
        }

        public override Memory Lookup(int Pointer)
        {
            return this._GetFile(Pointer).Open();
        }

        /// <summary>
        /// Gets the name of the file for the given index.
        /// </summary>
        private string _GetName(int Index)
        {
            return Index.ToString("X8");
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