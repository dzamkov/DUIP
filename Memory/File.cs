using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DUIP
{
    /// <summary>
    /// Memory for a file on the filesystem.
    /// </summary>
    public class FileMemory : Memory, IDisposable
    {
        public FileMemory(FileStream FileStream)
        {
            this._FileStream = FileStream;
        }

        public override InStream Read()
        {
            return new _InStream(0, this);
        }

        public override OutStream Modify(long Start)
        {
            return new _OutStream(Start, this);
        }

        public override long Size
        {
            get
            {
                return this._FileStream.Length;
            }
        }

        /// <summary>
        /// An input stream for file data.
        /// </summary>
        private class _InStream : InStream
        {
            public _InStream(long Position, FileMemory File)
            {
                this._Position = Position;
                this._File = File;
            }

            public override byte Read()
            {
                this._File._Seek(this._Position);
                this._Position++;
                return (byte)this._File._FileStream.ReadByte();
            }

            public override void Advance(long Amount)
            {
                this._Position += Amount;
            }

            public override void Read(byte[] Buffer, int Offset, int Length)
            {
                this._File._Seek(this._Position);
                this._Position += (long)Length;
                this._File._FileStream.Read(Buffer, Offset, Length);
            }

            private long _Position;
            private FileMemory _File;
        }

        /// <summary>
        /// An output stream for file data.
        /// </summary>
        private class _OutStream : OutStream
        {
            public _OutStream(long Position, FileMemory File)
            {
                this._File = File;
                this._Position = Position;
            }

            public override void Write(byte Data)
            {
                this._File._Seek(this._Position);
                this._File._FileStream.WriteByte(Data);
                this._Position++;
            }

            public override void Write(byte[] Buffer, int Offset, int Length)
            {
                this._File._Seek(this._Position);
                this._File._FileStream.Write(Buffer, Offset, Length);
                this._Position += Length;
            }

            public override void Advance(long Amount)
            {
                this._Position += Amount;
            }

            private long _Position;
            private FileMemory _File;
        }

        /// <summary>
        /// Insures the filestream is at the given position in the file.
        /// </summary>
        private void _Seek(long Position)
        {
            if (this._FileStream.Position != Position)
            {
                this._FileStream.Position = Position;
            }
        }

        public void Dispose()
        {
            this._FileStream.Close();
            this._FileStream.Dispose();
        }

        private FileStream _FileStream;
    }

    /// <summary>
    /// An input stream using a file as a source.
    /// </summary>
    public class FileInStream : InStream
    {
        public FileInStream(FileStream FileStream)
        {
            this._FileStream = FileStream;
        }

        /// <summary>
        /// Gets the native file stream used for reading.
        /// </summary>
        public FileStream FileStream
        {
            get
            {
                return this._FileStream;
            }
        }

        public override byte Read()
        {
            return (byte)this._FileStream.ReadByte();
        }

        public override void Advance(long Amount)
        {
            this._FileStream.Position += Amount;
        }

        public override void Read(byte[] Buffer, int Offset, int Length)
        {
            this._FileStream.Read(Buffer, Offset, Length);
        }

        public override void Finish()
        {
            this._FileStream.Close();
        }

        private FileStream _FileStream;
    }

    /// <summary>
    /// An output stream that writes to a file on the filesystem.
    /// </summary>
    public class FileOutStream : OutStream
    {
        public FileOutStream(FileStream FileStream)
        {
            this._FileStream = FileStream;
        }

        /// <summary>
        /// Gets the native file stream used for writing.
        /// </summary>
        public FileStream FileStream
        {
            get
            {
                return this._FileStream;
            }
        }

        public override void Write(byte Data)
        {
            this._FileStream.WriteByte(Data);
        }

        public override void Write(byte[] Buffer, int Offset, int Length)
        {
            this._FileStream.Write(Buffer, Offset, Length);    
        }

        public override void Advance(long Amount)
        {
            this._FileStream.Position += Amount;
        }

        public override void Finish()
        {
            this._FileStream.Close();
        }

        private FileStream _FileStream;
    }
}