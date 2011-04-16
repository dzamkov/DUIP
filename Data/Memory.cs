using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A simple stream that writes to a byte list.
    /// </summary>
    public class MemoryOutStream : OutStream
    {
        public MemoryOutStream(List<byte> Data)
        {
            this._Data = Data;
        }

        public MemoryOutStream()
        {
            this._Data = new List<byte>();
        }

        /// <summary>
        /// Gets the current data for the stream.
        /// </summary>
        public List<byte> Data
        {
            get
            {
                return this._Data;
            }
        }

        /// <summary>
        /// Creates an in stream to read the data written to this stream.
        /// </summary>
        public MemoryInStream Read
        {
            get
            {
                return new MemoryInStream(this._Data);
            }
        }

        public override void Write(byte Data)
        {
            this._Data.Add(Data);
        }

        private List<byte> _Data;
    }
    
    /// <summary>
    /// A simple stream that reads from a list of bytes.
    /// </summary>
    public class MemoryInStream : InStream
    {
        public MemoryInStream(List<byte> Data, int Index)
        {
            this._Data = Data;
            this._Index = Index;
        }

        public MemoryInStream(List<byte> Data)
        {
            this._Data = Data;
        }

        /// <summary>
        /// Gets the data that is read.
        /// </summary>
        public List<byte> Data
        {
            get
            {
                return this._Data;
            }
        }

        /// <summary>
        /// Gets the position in the byte list from which the next byte will be read from.
        /// </summary>
        public int Index
        {
            get
            {
                return this._Index;
            }
        }

        public override byte Read()
        {
            return this._Data[this._Index++];
        }

        public override void Advance(int Amount)
        {
            this._Index += Amount;
        }

        public override int BytesAvailable
        {
            get
            {
                return this._Data.Count - this._Index;
            }
        }

        private List<byte> _Data;
        private int _Index;
    }

    /// <summary>
    /// Data whose source is a byte list.
    /// </summary>
    public class MemoryData : Data
    {
        public MemoryData(List<byte> Source)
        {
            this._Source = Source;
        }

        /// <summary>
        /// Gets the source byte list for this data.
        /// </summary>
        public List<byte> Source
        {
            get
            {
                return this._Source;
            }
        }

        public override InStream Read()
        {
            return new MemoryInStream(this._Source);
        }

        public override InStream Read(int Start)
        {
            return new MemoryInStream(this._Source, Start);
        }

        public override int Length
        {
            get
            {
                return this._Source.Count;
            }
        }

        private List<byte> _Source;
    }
}