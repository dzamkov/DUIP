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
        public enum BlockState
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
            throw new NotImplementedException();
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
        /// Scheme information for a buddy allocator.
        /// </summary>
        public class Scheme
        {
            /// <summary>
            /// The size of the smallest possible block that can be allocated.
            /// </summary>
            public int BaseBlockSize;

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
                    int d = this.Depth;
                    int s = BlockHeaderSize + this.BaseBlockSize;
                    while (d-- > 0)
                    {
                        s = BlockHeaderSize + s + s;
                    }
                    return s;
                }
            }
        }

        private Memory _Source;
        private Scheme _Scheme;
    }
}