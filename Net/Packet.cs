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
    /// A component of a virtual connection that handles the assembly of received chunks into messages.
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
        /// Processes the receipt of a chunk. If a message can be formed with the chunk, this method returns true and
        /// Message is set.
        /// </summary>
        /// <param name="Data">The data for the chunk.</param>
        /// <param name="Final">Indicates wether the chunk is the initial one in a message.</param>
        /// <param name="Final">Indicates wether the chunk is the final one in a message.</param>
        public bool Receive(int SequenceNumber, Data Data, bool Initial, bool Final, ref Data Message)
        {
            // Find where this chunk fits in a message.
            _Chunk chunk;
            int first, last;
            bool hasinitial, hasfinal;
            if (Initial)
            {
                hasinitial = true;
                first = SequenceNumber;
            }
            else
            {
                if (this._Chunks.TryGetValue(SequenceNumber - 1, out chunk))
                {
                    first = chunk.First;
                    hasinitial = this._Chunks[first].Initial;
                }
                else
                {
                    first = SequenceNumber;
                    hasinitial = false;
                }
            }
            if (Final)
            {
                hasfinal = true;
                last = SequenceNumber;
            }
            else
            {
                if (this._Chunks.TryGetValue(SequenceNumber + 1, out chunk))
                {
                    last = chunk.Last;
                    hasfinal = this._Chunks[last].Final;
                }
                else
                {
                    last = SequenceNumber;
                    hasfinal = false;
                }
            }
            this._Chunks[SequenceNumber] = new _Chunk
            {
                Data = Data,
                Initial = Initial,
                Final = Final,
                First = first,
                Last = last
            };

            // See if a message can be created
            if (hasinitial && hasfinal)
            {
                bool remove; // Should the chunks be removed from the dictionary?
                if (this._AcknowledgementNumber == SequenceNumber)
                {
                    remove = true;
                    this._AcknowledgementNumber = last;
                    this._AcknowledgementNumber++;
                    this._UpdateAcknowledgement();
                }
                else
                {
                    remove = false;
                }

                if (first == last)
                {
                    Message = this._Process(first, remove);
                }
                else
                {
                    // Combine chunks into a single message
                    List<Data> parts = new List<Data>();
                    int t = first;
                    while (true)
                    {
                        parts.Add(this._Process(SequenceNumber, remove));
                        if (t != last)
                        {
                            t++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Message = Data.Concat(parts);
                }
                return true;
            }

            if (this._AcknowledgementNumber == SequenceNumber)
            {
                this._AcknowledgementNumber++;
                this._UpdateAcknowledgement();
            }
            return false;
        }

        /// <summary>
        /// Processes a chunk for a message.
        /// </summary>
        /// <param name="Remove">Indicates wether the chunk should be removed from the chunk registry (if the acknowledgement number is greater than the
        /// chunk's sequence number).</param>
        private Data _Process(int SequenceNumber, bool Remove)
        {
            _Chunk chunk = this._Chunks[SequenceNumber];
            Data data = chunk.Data;
            if (Remove)
            {
                this._Chunks.Remove(SequenceNumber);
            }
            else
            {
                // Indicate the chunk was processed without removing it from the registry.
                chunk.Data = null;
            }
            return data;
        }

        /// <summary>
        /// Handles an update in the acknowledgement number.
        /// </summary>
        private void _UpdateAcknowledgement()
        {
            _Chunk chunk;
            while (this._Chunks.TryGetValue(this._AcknowledgementNumber, out chunk))
            {
                // If the chunk has been processed, there is no reason to keep it in the registry
                if (chunk.Data == null)
                {
                    this._Chunks.Remove(this._AcknowledgementNumber);
                }
                this._AcknowledgementNumber++;
            }
        }

        /// <summary>
        /// Contains information about a received chunk.
        /// </summary>
        private class _Chunk
        {
            /// <summary>
            /// The data for the chunk. This will be null if the chunk was already processed into a message.
            /// </summary>
            public Data Data;

            /// <summary>
            /// Inidicates wether this chunk is the initial one in a message.
            /// </summary>
            public bool Initial;

            /// <summary>
            /// Indicates wether this chunk is the final one in a message.
            /// </summary>
            public bool Final;

            /// <summary>
            /// The sequence number for the newest chunk that could be in the same message as this chunk. This value is only correct
            /// for the last packet in a contiguous group of packets.
            /// </summary>
            public int First;

            /// <summary>
            /// The sequence number for the oldest chunk that could be in the same message as this chunk. This value is only correct
            /// for the first packet in a contiguous group of packets.
            /// </summary>
            public int Last;
        }

        private int _AcknowledgementNumber;
        private Dictionary<int, _Chunk> _Chunks;
    }

    /// <summary>
    /// A component of a virtual connection that breaks messages into chunks for sending.
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
        /// Queues a message with the given chunks to be sent.
        /// </summary>
        public void Send(IEnumerable<Data> Chunks)
        {
            bool initial = true;
            _Chunk last = null;
            foreach (Data data in Chunks)
            {
                _Chunk chunk = new _Chunk
                {
                    Data = data,
                    Initial = initial
                };
                initial = false;
                last = chunk;
                this._Chunks.AddLast(chunk);
            }
            last.Final = true;
        }

        /// <summary>
        /// Queues a message to be sent.
        /// </summary>
        public void Send(Data Message, long ChunkSize)
        {
            this.Send(Message.Break(ChunkSize));
        }

        /// <summary>
        /// Gets the next chunk to be sent by this terminal, or returns false if there are no more chunks to send.
        /// </summary>
        public bool Process(ref Data Data, ref bool Initial, ref bool Final)
        {
            if (this._SendNode == null)
            {
                return false;
            }
            else
            {
                _Chunk chunk = this._SendNode.Value;
                this._SendNode = this._SendNode.Next;
                Data = chunk.Data;
                Initial = chunk.Initial;
                Final = chunk.Final;
                return true;
            }
        }

        /// <summary>
        /// Sets the chunk with the given sequence number to be the next one sent.
        /// </summary>
        public void Reset(int SequenceNumber)
        {
            int off = SequenceNumber - this._AcknowledgementNumber;
            this._SendNode = this._Chunks.First;
            while (off-- > 0 && this._SendNode != null)
            {
                this._SendNode = this._SendNode.Next;
            }
        }

        /// <summary>
        /// Contains information about a chunk.
        /// </summary>
        private class _Chunk
        {
            /// <summary>
            /// The data for the chunk.
            /// </summary>
            public Data Data;

            /// <summary>
            /// Inidicates wether this chunk is the initial one in a message.
            /// </summary>
            public bool Initial;

            /// <summary>
            /// Indicates wether this chunk is the final one in a message.
            /// </summary>
            public bool Final;
        }

        private int _AcknowledgementNumber;
        private int _SequenceNumber;
        private LinkedListNode<_Chunk> _SendNode;
        private LinkedList<_Chunk> _Chunks;
    }
}