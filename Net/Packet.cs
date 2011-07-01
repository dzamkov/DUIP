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
        /// The packet has no explicit contents.
        /// </summary>
        Empty = 0x00,

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

        /// <summary>
        /// The packet contains a ping request. Once received, a packet with a PingResponse flag should be sent
        /// immediately (or not at all).
        /// </summary>
        PingRequest = 0x20,

        /// <summary>
        /// The packet is a response to a ping request.
        /// </summary>
        PingResponse = 0x40,

        /// <summary>
        /// The packet contains the current estimate for the round trip time for the connection.
        /// </summary>
        RoundTripTime = 0x80,
    }

    /// <summary>
    /// Information about the contents of a packet.
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// Writes a packet to a stream.
        /// </summary>
        public static void Write(Packet Packet, OutStream Stream)
        {
            // Build flags and header
            PacketFlags flags = PacketFlags.Empty;
            if (Packet.AcknowledgementNumber.HasValue)
                flags |= PacketFlags.Acknowledgement;
            if (Packet.RoundTripTime.HasValue)
                flags |= PacketFlags.RoundTripTime;
            if (Packet.ChunkData != null)
            {
                flags |= PacketFlags.Chunk;
                if (Packet.ChunkInitial)
                    flags |= PacketFlags.ChunkInitial;
                if (Packet.ChunkFinal)
                    flags |= PacketFlags.ChunkFinal;
            }
            if (Packet.PingRequest)
                flags |= PacketFlags.PingRequest;
            if (Packet.PingResponse)
                flags |= PacketFlags.PingResponse;
            if (Packet.Disconnect)
                flags |= PacketFlags.Disconnect;
            Stream.WriteByte((byte)flags);
            Stream.WriteInt(Packet.SequenceNumber);
            
            // Additional information
            int ack;
            if (Packet.AcknowledgementNumber.TryGetValue(out ack))
                Stream.WriteInt(ack);

            double rtt;
            if (Packet.RoundTripTime.TryGetValue(out rtt))
                Stream.WriteDouble(rtt);

            // Chunk
            if (Packet.ChunkData != null)
                Stream.Write(Packet.ChunkData, 0, Packet.ChunkData.Length);
        }

        /// <summary>
        /// Reads a packet from a stream with the given size in bytes or returns null if the packet cannot be parsed.
        /// </summary>
        public static Packet Read(InStream Stream, int Size)
        {
            Packet packet = new Packet();

            // Read flags and header
            if ((Size -= StreamSize.Byte + StreamSize.Int) < 0)
                return null;
            PacketFlags flags = (PacketFlags)Stream.ReadByte();
            packet.SequenceNumber = Stream.ReadInt();
            packet.PingRequest = (flags & PacketFlags.PingRequest) == PacketFlags.PingRequest;
            packet.PingResponse = (flags & PacketFlags.PingResponse) == PacketFlags.PingResponse;

            // Read additional information
            if ((flags & PacketFlags.Acknowledgement) == PacketFlags.Acknowledgement)
            {
                if ((Size -= StreamSize.Int) < 0)
                    return null;
                packet.AcknowledgementNumber = Stream.ReadInt();
            }
            if ((flags & PacketFlags.RoundTripTime) == PacketFlags.RoundTripTime)
            {
                if ((Size -= StreamSize.Double) < 0)
                    return null;
                packet.RoundTripTime = Stream.ReadDouble();
            }

            // Read chunk if any
            if ((flags & PacketFlags.Chunk) == PacketFlags.Chunk)
            {
                packet.ChunkInitial = (flags & PacketFlags.ChunkInitial) == PacketFlags.ChunkInitial;
                packet.ChunkFinal = (flags & PacketFlags.ChunkFinal) == PacketFlags.ChunkFinal;

                byte[] data = new byte[Size];
                Stream.Read(data, 0, data.Length);
                packet.ChunkData = data;

                return packet;
            }
            else
            {
                // A packet can only be a disconnect if it does not have a chunk
                packet.Disconnect = (flags & PacketFlags.Disconnect) == PacketFlags.Disconnect;

                // Make sure this is the end of the packet
                if (Size == 0)
                    return packet;
                else
                    return null;
            }
        }

        /// <summary>
        /// The sequence number for the packet. If the packet contains a chunk, then this is the sequence number for that chunk. If there is no
        /// chunk data, this number is used to validate the packet.
        /// </summary>
        public int SequenceNumber;

        /// <summary>
        /// An optional acknowledgement number that gives the sequence number of the next chunk expected from the receiver of the packet.
        /// </summary>
        public Maybe<int> AcknowledgementNumber;

        /// <summary>
        /// An optional field given an estimate of the round trip time of communication channel.
        /// </summary>
        public Maybe<double> RoundTripTime;

        /// <summary>
        /// Data for the chunk included with the packet, or null if the packet does not contain a chunk.
        /// </summary>
        public byte[] ChunkData;

        /// <summary>
        /// Indicates wether the chunk for this packet is the initial one in a message.
        /// </summary>
        public bool ChunkInitial;

        /// <summary>
        /// Indicates wether the chunk for this packet is the final one in a message.
        /// </summary>
        public bool ChunkFinal;

        /// <summary>
        /// Indicates wether this packet is a ping request.
        /// </summary>
        public bool PingRequest;

        /// <summary>
        /// Indicates wether this packet is a ping response.
        /// </summary>
        public bool PingResponse;

        /// <summary>
        /// Indicates wether this packet signals a disconnect.
        /// </summary>
        public bool Disconnect;
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
        /// <param name="Acknowledged">Indicates wether the receipt of this chunk has changed the acknowledgement number.</param>
        public bool Process(int SequenceNumber, byte[] Data, bool Initial, bool Final, out bool Acknowledged, ref Disposable<InStream> Message)
        {
            _Chunk chunk;

            // Make sure we need this chunk
            if (this._Chunks.ContainsKey(SequenceNumber))
            {
                Acknowledged = false;
                return false;
            }

            // Update acknowledgement number
            if (Acknowledged = this._AcknowledgementNumber == SequenceNumber)
            {
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
                Message = new _ReceiveStream(firstsq, Acknowledged, this);
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
        private class _ReceiveStream : InStream, IDisposable
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

                    if (current.Final)
                    {
                        break;
                    }
                    else
                    {
                        current = this._Chunks[++sq];
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
                LinkedListNode<_Chunk> node;
                while (this._AcknowledgementNumber != value && (node = this._Chunks.First) != null)
                {
                    if (node == this._SendNode)
                    {
                        this._SendNode = null;
                    }
                    this._Chunks.RemoveFirst();
                    this._AcknowledgementNumber++;
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