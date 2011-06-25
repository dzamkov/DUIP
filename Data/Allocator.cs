﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A representation of storage space that allows the allocation and deallocation of fixed-size mutable data.
    /// </summary>
    /// <typeparam name="T">A reference to allocated data in the allocator.</typeparam>
    public abstract class Allocator<T> : Store<T>
    {
        /// <summary>
        /// Allocates data with the given size. The resulting data will be null if there is no
        /// space for allocation available.
        /// </summary>
        public abstract T Allocate(long Size, out Data Data);

        /// <summary>
        /// Stores the provided data somewhere in the allocator and gives a reference to the data in
        /// the allocator. The resulting data will be null if there is no space for allocation available.
        /// </summary>
        public virtual T Store(ref Data Data)
        {
            Data k;
            T r = this.Allocate(Data.Length, out k);
            if (k != null)
            {
                k.Modify().Write(Data.Read());
                Data = k;
                return r;
            }
            Data = null;
            return r;
        }

        /// <summary>
        /// Deallocates the data with the given reference. After this is called, the corresponding data
        /// may no longer be used.
        /// </summary>
        public abstract void Deallocate(T Reference);
    }

    /// <summary>
    /// An allocator that stores data as files in a directory. Data in this kind of allocator will persist between
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

        public override int Allocate(long Size, out Data Data)
        {
            int rand;
            Path fpath;
            do
            {
                rand = this._Random.Next(int.MinValue, int.MaxValue);
                fpath = this._GetFile(rand);
            }
            while (fpath.FileExists);
            Data = fpath.Create(Size);
            return rand;
        }

        public override Data Lookup(int Reference)
        {
            return this._GetFile(Reference).Open();
        }

        public override void Deallocate(int Reference)
        {
            this._GetFile(Reference).Delete();
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