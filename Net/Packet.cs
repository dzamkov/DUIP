using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Net
{
    /// <summary>
    /// Describes the contents of a packet.
    /// </summary>
    [Flags]
    public enum PacketFlags : byte
    {
        /// <summary>
        /// The packet contains part of a message.
        /// </summary>
        Chunk = 0x01,

        /// <summary>
        /// The packet requests a connection with another peer.
        /// </summary>
        ConnectionRequest = 0x02,

        /// <summary>
        /// The packet is a response to a connection request that causes a connection to be opened.
        /// </summary>
        ConnectionAccept = 0x03,

        /// <summary>
        /// The packet is a negative response to a connection request that indicates that a connection may not be opened.
        /// </summary>
        ConnectionRefuse = 0x04,

        /// <summary>
        /// The packet informs the receiver that the connection has been closed and no more message may be sent or received.
        /// </summary>
        Disconnect = 0x02,

        /// <summary>
        /// The chunk contained within the packet is the first one in a message.
        /// </summary>
        ChunkInitial = 0x02,

        /// <summary>
        /// The chunk contained within the packet is the last one in a message.
        /// </summary>
        ChunkFinal = 0x04,

        /// <summary>
        /// The packet contains an acknowledgement number.
        /// </summary>
        Acknowledgement = 0x10,
    }

    /// <summary>
    /// Contains functions that allow the processing and creation of packets.
    /// </summary>
    public static class Packet
    {

    }

    /// <summary>
    /// A component of a virtual connection that handles the assembly of received chunks into a stream.
    /// </summary>
    public class InTerminal
    {
        public InTerminal(int InitialSequenceNumber)
        {
            this._AcknowledgementNumber = InitialSequenceNumber;
            this._Chunks = new Dictionary<int, _Chunk>();
        }

        /// <summary>
        /// Gets the acknowledgement number (the sequence number for the next chunk needed by the terminal).
        /// </summary>
        public int AcknowledgementNumber
        {
            get
            {
                return this._AcknowledgementNumber;
            }
        }

        /// <summary>
        /// Processes the receipt of a chunk. Returns true and sets the message stream if a full message has been received.
        /// </summary>
        /// <param name="SequenceNumber">The sequence number for the received chunk.</param>
        public bool Process(int SequenceNumber, byte[] Data, bool Initial, bool Final, ref Disposable<InStream> Message)
        {
            _Chunk chunk;

            // Make sure we need this chunk
            if (this._Chunks.ContainsKey(SequenceNumber))
            {
                return false;
            }

            // Update acknowledgement number
            bool remove = false;
            if (this._AcknowledgementNumber == SequenceNumber)
            {
                remove = true;
                while (this._Chunks.TryGetValue(++this._AcknowledgementNumber, out chunk))
                {
                    // If the chunk has already been read in a message, remove it
                    if (chunk.Data == null)
                    {
                        this._Chunks.Remove(this._AcknowledgementNumber);
                    }
                }
            }

            // Add chunk
            chunk = new _Chunk
            {
                Data = Data,
                Initial = Initial,
                Final = Final
            };

            // Find "First" chunk
            _Chunk first;
            int firstsq = SequenceNumber - 1;
            if (!Initial && this._Chunks.TryGetValue(firstsq, out first))
            {
                firstsq = first.First;
                first = this._Chunks[firstsq];
                chunk.First = firstsq;
            }
            else
            {
                chunk.First = firstsq = SequenceNumber;
                first = chunk;
            }

            // Find "Last" chunk
            _Chunk last;
            int lastsq = SequenceNumber + 1;
            if (!Final && this._Chunks.TryGetValue(lastsq, out last))
            {
                lastsq = last.Last;
                last = this._Chunks[lastsq];
                chunk.Last = lastsq;
            }
            else
            {
                chunk.Last = lastsq = SequenceNumber;
                last = chunk;
            }

            // Check if a full message can be formed
            this._Chunks[SequenceNumber] = chunk;
            if (first.Initial && last.Final)
            {
                Message = new _ReceiveStream(firstsq, remove, this);
                return true;
            }
            else
            {
                first.Last = lastsq;
                last.First = firstsq;
                return false;
            }
        }

        /// <summary>
        /// A stream that reads assembled messages from a terminal.
        /// </summary>
        private class _ReceiveStream : InStream
        {
            public _ReceiveStream(int SequenceNumber, bool Remove, InTerminal Terminal)
            {
                this._Remove = Remove;
                this._Chunks = Terminal._Chunks;
                this._SequenceNumber = SequenceNumber;
                this._Current = this._Chunks[SequenceNumber];
            }

            public override byte Read()
            {
                byte[] data;
                while ((data = this._Current.Data).Length - this._Offset < 1)
                {
                    this._Continue();
                }
                return data[this._Offset++];
            }

            /// <summary>
            /// Advances to the next chunk.
            /// </summary>
            private void _Continue()
            {
                if (this._Current.Final)
                {
                    throw new StreamUnderflowException();
                }
                if (this._Remove)
                {
                    this._Chunks.Remove(this._SequenceNumber);
                }
                else
                {
                    this._Current.Data = null;
                }
                this._Offset = 0;
                this._Current = this._Chunks[++this._SequenceNumber];
            }

            public void Dispose()
            {
                _Chunk current = this._Current;
                int sq = this._SequenceNumber;
                while (true)
                {
                    if (this._Remove)
                    {
                        this._Chunks.Remove(sq);
                    }
                    else
                    {
                        current.Data = null;
                    }
                    current = this._Chunks[++sq];
                    if (current.Final)
                    {
                        break;
                    }
                }
            }

            private bool _Remove;
            private int _Offset;
            private int _SequenceNumber;
            private _Chunk _Current;
            private Dictionary<int, _Chunk> _Chunks;
        }

        /// <summary>
        /// Information about a chunk received by the terminal.
        /// </summary>
        private class _Chunk
        {
            /// <summary>
            /// The data for the chunk.
            /// </summary>
            public byte[] Data;

            /// <summary>
            /// Indicates wether the chunk is the initial one in a message.
            /// </summary>
            public bool Initial;

            /// <summary>
            /// Indicates wether the chunk is the final one in a message.
            /// </summary>
            public bool Final;

            /// <summary>
            /// The chunk with the lowest possible sequence number such that the chunk is in the same message as this chunk and all chunks
            /// between these have been received.
            /// </summary>
            /// <remarks>If a chunk is marked with Final and the chunk at First is marked with Initial, the span between
            /// the two (inclusive) is a complete message.</remarks>
            public int First;

            /// <summary>
            /// The chunk with the highest possible sequence number such that the chunk is in the same message as this chunk and all chunks
            /// between these have been received.
            /// </summary>
            public int Last;
        }

        private int _AcknowledgementNumber;
        private Dictionary<int, _Chunk> _Chunks;
    }

    /// <summary>
    /// A component of a virtual connection that breaks stream data into chunks for sending.
    /// </summary>
    public class OutTerminal
    {
        public OutTerminal(int InitialSequenceNumber)
        {
            this._SequenceNumber = InitialSequenceNumber;
            this._AcknowledgementNumber = InitialSequenceNumber;
            this._Chunks = new LinkedList<_Chunk>();
        }

        /// <summary>
        /// Gets the sequence number to use for the next chunk.
        /// </summary>
        public int SequenceNumber
        {
            get
            {
                return this._SequenceNumber;
            }
        }

        /// <summary>
        /// Gets or sets the known acknowledgement number from the receiving terminal.
        /// </summary>
        public int AcknowledgementNumber
        {
            get
            {
                return this._AcknowledgementNumber;
            }
            set
            {
                while (this._AcknowledgementNumber++ != value)
                {
                    this._Chunks.RemoveFirst();
                }
            }
        }

        /// <summary>
        /// Gives a stream that allows the sending of a message from this terminal. Only one such send stream may be open at a time.
        /// </summary>
        /// <param name="ChunkSize">The maximum size of the chunks to break the message into.</param>
        public Disposable<OutStream> Send(int ChunkSize)
        {
            return new _SendStream(ChunkSize, this);
        }

        /// <summary>
        /// A stream that writes a message to a terminal for sending.
        /// </summary>
        private class _SendStream : OutStream, IDisposable
        {
            public _SendStream(int ChunkSize, OutTerminal Terminal)
            {
                this._ChunkSize = ChunkSize;
                this._Terminal = Terminal;
                this._Current = new _Chunk
                {
                    Data = new byte[ChunkSize],
                    Initial = true
                };
            }


            public override void Write(byte Data)
            {
                byte[] data;
                while ((data = this._Current.Data).Length - this._Offset < 1)
                {
                    this._NextChunk();
                }
                data[this._Offset] = Data;
                this._Offset++;
            }

            /// <summary>
            /// Adds the current chunk to the terminal and starts a new chunk.
            /// </summary>
            private void _NextChunk()
            {
                this._Terminal._Push(this._Current);
                this._Current = new _Chunk()
                {
                    Data = new byte[this._ChunkSize]
                };
            }
            public void Dispose()
            {
                // Remove extra data
                byte[] sdata = this._Current.Data;
                byte[] ndata = new byte[this._Offset];
                for (int t = 0; t < ndata.Length; t++)
                {
                    ndata[t] = sdata[t];
                }
                this._Current.Data = ndata;

                // Set the current chunk as final
                this._Current.Final = true;

                // Final push
                this._Terminal._Push(this._Current);
            }

            private int _ChunkSize;
            private int _Offset;
            private _Chunk _Current;
            private OutTerminal _Terminal;
        }

        /// <summary>
        /// Appends a new chunk to this terminal.
        /// </summary>
        private void _Push(_Chunk Chunk)
        {
            lock (this)
            {
                LinkedListNode<_Chunk> node = this._Chunks.AddLast(Chunk);
                if (this._SendNode == null)
                {
                    this._SendNode = node;
                    this._SendNumber = this._SequenceNumber;
                }
                this._SequenceNumber++;
            }
        }

        /// <summary>
        /// Gets the next chunk to be sent by this terminal, or returns false if there are no more chunks to send.
        /// </summary>
        public bool Process(ref int SequenceNumber, ref byte[] Data, ref bool Initial, ref bool Final)
        {
            if (this._SendNode == null)
            {
                return false;
            }
            else
            {
                lock (this)
                {
                    _Chunk chunk = this._SendNode.Value;
                    this._SendNode = this._SendNode.Next;
                    SequenceNumber = this._SendNumber++;
                    Data = chunk.Data;
                    Initial = chunk.Initial;
                    Final = chunk.Final;
                    return true;
                }
            }
        }

        /// <summary>
        /// Sets the chunk with the given sequence number to be the next one sent.
        /// </summary>
        public void Reset(int SequenceNumber)
        {
            this._SendNumber = SequenceNumber;
            int off = SequenceNumber - this._AcknowledgementNumber;
            this._SendNode = this._Chunks.First;
            while (off-- > 0 && this._SendNode != null)
            {
                this._SendNode = this._SendNode.Next;
            }
        }

        /// <summary>
        /// Information about a chunk to be sent by the terminal.
        /// </summary>
        private class _Chunk
        {
            /// <summary>
            /// The data for the chunk.
            /// </summary>
            public byte[] Data;

            /// <summary>
            /// Indicates wether the chunk is the initial one in a message.
            /// </summary>
            public bool Initial;

            /// <summary>
            /// Indicates wether the chunk is the final one in a message.
            /// </summary>
            public bool Final;
        }

        private int _AcknowledgementNumber;
        private int _SequenceNumber;
        private int _SendNumber;
        private LinkedListNode<_Chunk> _SendNode;
        private LinkedList<_Chunk> _Chunks;
    }
}