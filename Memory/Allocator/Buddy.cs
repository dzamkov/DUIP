using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Memory
{
    /// <summary>
    /// An implementation of a buddy allocator that fully contains its state, and allocated memory(s), in a single memory object.
    /// </summary>
    /// <remarks>This is not an ideal implementation. It is possible for fragmentation to occur that results in blocks of larger sizes
    /// to not be able to be allocated. This implementation works best when allocations are given in descending order of size.</remarks>
    public class BuddyAllocator : Allocator<long>, IHandle
    {
        private BuddyAllocator()
        {

        }

        /// <summary>
        /// The size of a block header in bytes.
        /// </summary>
        public const long BlockHeaderSize = 1;

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

        public override Memory Allocate(long Size, out long Pointer)
        {
            Pointer = 0;
            Scheme scheme = this._Scheme;
            long csize = this._ContentSize;
            if (Size > csize)
            {
                return null;
            }


            BlockState state;
            if (_Allocate(this._Source, 0, csize, scheme.Depth, Size, ref Pointer, out state))
            {
                return this.Lookup(Pointer);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tries allocating memory in the block with the given parameters.
        /// </summary>
        /// <param name="Source">The memory that contains the block to be used for allocation.</param>
        /// <param name="Start">A pointer to the beginning of the block (including header).</param>
        /// <param name="ContentSize">The size of the content of the block.</param>
        /// <param name="Depth">The depth of the block.</param>
        /// <param name="State">The state of the block after the function exits.</param>
        /// <param name="Size">The size of the memory to allocate.</param>
        private static bool _Allocate(Memory Source, long Start, long ContentSize, int Depth, long Size, ref long Pointer, out BlockState State)
        {
            State = _GetBlockState(Source, Start);
            long ibcs; // Content size for inner blocks.
            switch (State)
            {
                case BlockState.Empty:
                    ibcs = ContentSize / 2 - BlockHeaderSize;
                    if (Size > ibcs || Depth == 0)
                    {
                        Pointer = Start + BlockHeaderSize;
                        _SetBlockState(Source, Start, State = BlockState.Filled);
                        return true;
                    }

                    State = BlockState.Split;
                    long cur = Start;
                    while (true)
                    {
                        _SetBlockState(Source, cur, BlockState.Split);
                        cur += BlockHeaderSize;

                        _SetBlockState(Source, cur + BlockHeaderSize + ibcs, BlockState.Empty);

                        ibcs = ibcs / 2 - BlockHeaderSize;
                        if (Size > ibcs || --Depth == 0)
                        {
                            Pointer = cur + BlockHeaderSize;
                            _SetBlockState(Source, cur, BlockState.Filled);
                            return true;
                        }
                    }
                case BlockState.Split:
                    ibcs = ContentSize / 2 - BlockHeaderSize;
                    if (Size <= ibcs)
                    {
                        int ndepth = Depth - 1;
                        long fstart = Start + BlockHeaderSize;
                        long sstart = fstart + BlockHeaderSize + ibcs;
                        BlockState fstate, sstate;
                        if (_Allocate(Source, fstart, ibcs, ndepth, Size, ref Pointer, out fstate))
                        {
                            sstate = _GetBlockState(Source, sstart);
                            if (_Filled(fstate) && _Filled(sstate))
                            {
                                _SetBlockState(Source, Start, State = BlockState.FilledSplit);
                            }
                            return true;
                        }
                        else
                        {
                            if (_Allocate(Source, sstart, ibcs, ndepth, Size, ref Pointer, out sstate))
                            {
                                if (_Filled(fstate) && _Filled(sstate))
                                {
                                    _SetBlockState(Source, Start, State = BlockState.FilledSplit);
                                }
                                return true;
                            }
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }

        public override void Deallocate(long Size, long Pointer)
        {
            this.Deallocate(Pointer);
        }

        /// <summary>
        /// Deallocates the memory beginning at the given pointer.
        /// </summary>
        public void Deallocate(long Pointer)
        {
            BlockState state;
            _Deallocate(this._Source, 0, this._ContentSize, Pointer, out state);
        }

        /// <summary>
        /// Deallocates a pointer in the given block.
        /// </summary>
        /// <param name="State">The state of the block after the function exits.</param>
        private static void _Deallocate(Memory Source, long Start, long ContentSize, long Pointer, out BlockState State)
        {
            if (Pointer == Start + BlockHeaderSize)
            {
                _SetBlockState(Source, Start, State = BlockState.Empty);
                return;
            }

            long ibbs = ContentSize / 2;
            long ibcs = ibbs - BlockHeaderSize;
            long fstart = Start + BlockHeaderSize;
            long sstart = fstart + BlockHeaderSize + ibcs;
            BlockState fstate, sstate;
            if (Pointer - fstart < ibbs)
            {
                _Deallocate(Source, fstart, ibcs, Pointer, out fstate);
                sstate = _GetBlockState(Source, sstart);
                
            }
            else
            {
                fstate = _GetBlockState(Source, fstart);
                _Deallocate(Source, sstart, ibcs, Pointer, out sstate);
            }
            if (fstate == BlockState.Empty && sstate == BlockState.Empty)
            {
                _SetBlockState(Source, Start, State = BlockState.Empty);
            }
            else
            {
                _SetBlockState(Source, Start, State = BlockState.Split);
            }
        }

        public override Memory Lookup(long Pointer)
        {
            return this._Source.GetPartion(Pointer, this._Source.Size - Pointer);
        }

        /// <summary>
        /// Gets the state of a block in memory.
        /// </summary>
        /// <param name="Start">A pointer to the beginning of the block (including header).</param>
        private static BlockState _GetBlockState(Memory Source, long Start)
        {
            BlockState state;
            using (Disposable<InStream> str = Source.Read(Start))
            {
                state = (BlockState)str.Object.ReadByte();
            }
            return state;
        }

        /// <summary>
        /// Sets the state of a block in memory.
        /// </summary>
        /// <param name="Start">A pointer to the beginning of the block (including header).</param>
        private static void _SetBlockState(Memory Source, long Start, BlockState State)
        {
            using (Disposable<OutStream> str = Source.Write(Start))
            {
                str.Object.WriteByte((byte)State);
            }
        }

        /// <summary>
        /// Gets if the given state is either Filled, or FilledSplit.
        /// </summary>
        private static bool _Filled(BlockState State)
        {
            return State == BlockState.Filled || State == BlockState.FilledSplit;
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
            using (Disposable<OutStream> os = Source.Write())
            {
                os.Object.WriteByte((byte)BlockState.Empty);
            }

            // Return allocator
            return new BuddyAllocator
            {
                _Source = Source,
                _Scheme = Scheme,
                _ContentSize = GetContentSize(Scheme.BaseContentSize, Scheme.Depth)
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
                _Scheme = Scheme,
                _ContentSize = GetContentSize(Scheme.BaseContentSize, Scheme.Depth)
            };
        }

        /// <summary>
        /// Gets the content size of a block given the base content size and the recursive depth of the block.
        /// </summary>
        public static long GetContentSize(long BaseContentSize, int Depth)
        {
            while (Depth-- > 0)
            {
                long blocksize =  BlockHeaderSize + BaseContentSize;
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
            public long BaseContentSize;

            /// <summary>
            /// The maximum possible recusive depth for blocks.
            /// </summary>
            public int Depth;

            /// <summary>
            /// Gets the total size needed for a buddy allocator using this scheme.
            /// </summary>
            public long RequiredSize
            {
                get
                {
                    return BlockHeaderSize + GetContentSize(this.BaseContentSize, this.Depth);
                }
            }
        }

        private long _ContentSize;
        private Memory _Source;
        private Scheme _Scheme;
    }
}