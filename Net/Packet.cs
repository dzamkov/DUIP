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
    public class InTerminal : InStream
    {
        public InTerminal(int InitialSequenceNumber)
        {
            this._AcknowledgementNumber = InitialSequenceNumber;
            this._ReadNumber = this._AcknowledgementNumber;
            this._Chunks = new Dictionary<int, byte[]>();
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
        /// Gets the sequence number of the next chunk to be read.
        /// </summary>
        public int ReadNumber
        {
            get
            {
                return this._ReadNumber;
            }
        }

        /// <summary>
        /// Processes the receipt of a chunk.
        /// </summary>
        /// <param name="SequenceNumber">The sequence number for the received chunk.</param>
        public void Receive(int SequenceNumber, byte[] Chunk)
        {
            if (this._AcknowledgementNumber == SequenceNumber)
            {
                this._AcknowledgementNumber++;
                while (this._Chunks.ContainsKey(this._AcknowledgementNumber))
                {
                    this._AcknowledgementNumber++;
                }
            }
            this._Chunks[SequenceNumber] = Chunk;
        }

        public override byte Read()
        {
            throw new NotImplementedException();
        }

        private int _AcknowledgementNumber;
        private int _ReadNumber;
        private int _Offset;
        private byte[] _Current;
        private Dictionary<int, byte[]> _Chunks;
    }

    /// <summary>
    /// A component of a virtual connection that breaks stream data into chunks for sending.
    /// </summary>
    public class OutTerminal : OutStream
    {
        public OutTerminal(int InitialSequenceNumber, int ChunkSize)
        {
            this._SequenceNumber = InitialSequenceNumber;
            this._AcknowledgementNumber = InitialSequenceNumber;
            this._Chunks = new LinkedList<byte[]>();
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
        /// Gets the maximum size for a chunk sent by this terminal.
        /// </summary>
        public int ChunkSize
        {
            get
            {
                return this._ChunkSize;
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
        /// Gets the next chunk to be sent by this terminal, or returns false if there are no more chunks to send.
        /// </summary>
        public bool Process(ref byte[] Data)
        {
            if (this._SendNode == null)
            {
                return false;
            }
            else
            {
                Data = this._SendNode.Value;
                this._SendNode = this._SendNode.Next;
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
        /// Insures all written data is assigned to chunks and returns the sequence number for the final chunk
        /// with data.
        /// </summary>
        public int Flush()
        {
            throw new NotImplementedException();
        }

        public override void Write(byte Data)
        {
            throw new NotImplementedException();
        }

        private int _AcknowledgementNumber;
        private int _SequenceNumber;
        private int _ChunkSize;
        private LinkedListNode<byte[]> _SendNode;
        private LinkedList<byte[]> _Chunks;
    }
}