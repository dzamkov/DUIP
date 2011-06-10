using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An implementation of a buddy allocator that fully contains its state, and allocated memory(s), in a single memory object.
    /// </summary>
    public class BuddyAllocator : Allocator<int>, IHandle
    {
        private BuddyAllocator()
        {

        }

        /// <summary>
        /// The size of a block header in bytes.
        /// </summary>
        public const int BlockHeaderSize = 1;

        /// <summary>
        /// Gives a possible state for a block in the allocator.
        /// </summary>
        public enum BlockState : byte
        {
            /// <summary>
            /// The block is completely empty
            /// </summary>
            Empty,

            /// <summary>
            /// The block is split into parts, and one of the parts has allocation
            /// space available.
            /// </summary>
            Split,

            /// <summary>
            /// The block is filled with contiguous allocated data.
            /// </summary>
            Filled,
            
            /// <summary>
            /// The block is split into parts, and both parts have a state of
            /// Filled or FilledSplit.
            /// </summary>
            FilledSplit,
        }

        public override Memory Allocate(long Size, out int Pointer)
        {
            Pointer = 0;
            int isize;
            try
            {
                isize = checked((int)Size);
            }
            catch (ArithmeticException)
            {
                return null;
            }

            Scheme scheme = this._Scheme;
            int csize = GetContentSize(scheme.BaseContentSize, scheme.Depth);
            if(isize > csize)
            {
                return null;
            }

            
            Memory mem = null;
            InStream str = this._Source.Read();
            if (!_Allocate(isize, str, csize, scheme.Depth, 0, ref Pointer, ref mem))
            {
                str.Finish();
            }
            return mem;
        }

        /// <summary>
        /// Tries allocating memory in the block with the given parameters.
        /// </summary>
        /// <param name="Size">The size of the memory to allocate. This should be at, or smaller than the content size of the block.</param>
        /// <param name="Stream">A stream containing the block. The stream is closed if memory is allocated.</param>
        /// <param name="ContentSize">The size of the content of the block in the stream.</param>
        /// <param name="Depth">The depth of the block.</param>
        /// <param name="Start">The pointer to the current position of the stream</param>
        private static bool _Allocate(int Size, InStream Stream, int ContentSize, int Depth, int Start, ref int Pointer, ref Memory Memory)
        {
            switch ((BlockState)Stream.ReadByte())
            {
                case BlockState.Empty:
                    throw new NotImplementedException();
                case BlockState.Split:
                    int ibcs = ContentSize / 2 - BlockHeaderSize;
                    if (Size > ibcs)
                    {
                        Stream.Advance(ContentSize);
                        return false;
                    }
                    return
                        _Allocate(Size, Stream, ibcs, Depth - 1, Start + BlockHeaderSize, ref Pointer, ref Memory) ||
                        _Allocate(Size, Stream, ibcs, Depth - 1, Start + BlockHeaderSize + ibcs, ref Pointer, ref Memory);
                default:
                    Stream.Advance(ContentSize);
                    return false;
            }
        }

        public override void Deallocate(long Size, int Pointer)
        {
            throw new NotImplementedException();
        }

        public override Memory Lookup(int Pointer)
        {
            throw new NotImplementedException();
        }

        public Memory Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Initializes a new buddy allocator in the given memory. The size of the memory must be
        /// at least the required size computed by the scheme.
        /// </summary>
        public static BuddyAllocator Create(Memory Source, Scheme Scheme)
        {
            // Initialize the first block in the allocator to empty
            OutStream os = Source.Modify();
            os.WriteByte((byte)BlockState.Empty);
            os.Finish();

            // Return allocator
            return new BuddyAllocator
            {
                _Source = Source,
                _Scheme = Scheme
            };
        }

        /// <summary>
        /// Loads a buddy allocator from the given memory using the given scheme.
        /// </summary>
        public static BuddyAllocator Restore(Memory Source, Scheme Scheme)
        {
            return new BuddyAllocator
            {
                _Source = Source,
                _Scheme = Scheme
            };
        }

        /// <summary>
        /// Gets the content size of a block given the base content size and the recursive depth of the block.
        /// </summary>
        public static int GetContentSize(int BaseContentSize, int Depth)
        {
            while (Depth-- > 0)
            {
                int blocksize =  BlockHeaderSize + BaseContentSize;
                BaseContentSize = blocksize + blocksize;
            }
            return BaseContentSize;
        }

        /// <summary>
        /// Scheme information for a buddy allocator.
        /// </summary>
        public class Scheme
        {
            /// <summary>
            /// The size of the smallest possible allocatable partion.
            /// </summary>
            public int BaseContentSize;

            /// <summary>
            /// The maximum possible recusive depth for blocks.
            /// </summary>
            public int Depth;

            /// <summary>
            /// Gets the total size needed for a buddy allocator using this scheme.
            /// </summary>
            public int RequiredSize
            {
                get
                {
                    return BlockHeaderSize + GetContentSize(this.BaseContentSize, this.Depth);
                }
            }
        }

        private Memory _Source;
        private Scheme _Scheme;
    }
}