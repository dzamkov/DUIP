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
        /// The packet tests if the connection is valid, and gives the initial sequence number used by the receiving terminal.
        /// </summary>
        Handshake = 0x02,

        /// <summary>
        /// The packet informs the receiver that the connection has been closed and no more message may be sent or received.
        /// </summary>
        Disconnect = 0x03,

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
        public int AcknowledgmentNumber
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
            // Move acknowledgement number up, if needed
            _Chunk chunk;
            if (this._AcknowledgementNumber == SequenceNumber)
            {
                this._AcknowledgementNumber++;
                while (this._Chunks.TryGetValue(this._AcknowledgementNumber, out chunk))
                {
                    if (chunk.Data == null)
                    {
                        this._Chunks.Remove(this._AcknowledgementNumber);
                    }
                    this._AcknowledgementNumber++;
                }
            }

            // Find where this chunk fits in a message.
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
                bool remove = this._AcknowledgementNumber > last; // Should the chunks be removed from the dictionary?
                if (first == last)
                {
                    chunk = this._Chunks[first];
                    Message = chunk.Data;
                    if (remove)
                    {
                        this._Chunks.Remove(first);
                    }
                    else
                    {
                        chunk.Data = null;
                    }
                }
                else
                {
                    // Combine chunks into a single message
                    List<Data> parts = new List<Data>();
                    int t = first;
                    while (true)
                    {
                        chunk = this._Chunks[t];
                        parts.Add(chunk.Data);
                        if (remove)
                        {
                            this._Chunks.Remove(t);
                        }
                        else
                        {
                            // Indicate that the chunk was processed
                            chunk.Data = null;
                        }

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
            return false;
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
            /// Indicates wether this chunk is the final one in a message.
            /// </summary>
            public bool Final;

            /// <summary>
            /// Inidicates wether this chunk is the initial one in a message.
            /// </summary>
            public bool Initial;

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

    }
}