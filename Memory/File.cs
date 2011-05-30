using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DUIP
{
    /// <summary>
    /// Memory for a file on the filesystem.
    /// </summary>
    public class FileMemory : Memory
    {
        private FileMemory(Path Path)
        {
            this._Path = Path;
        }

        /// <summary>
        /// Gets the file memory for the file at the given path. Returns null if there is a problem opening the file.
        /// </summary>
        public static FileMemory Open(Path Path)
        {
            FileMemory fm;
            if (!_Files.TryGetValue(Path, out fm))
            {
                fm = new FileMemory(Path);
                if (!fm.Open())
                {
                    return null;
                }
                _Files[Path] = fm;
            }
            return fm;
        }

        /// <summary>
        /// Creates a file at the given path and return file memory for it. Returns null if there is a problem creating the file.
        /// </summary>
        public static FileMemory Create(Path Path, long Size)
        {
            try
            {
                FileStream fs = System.IO.File.Open(Path, FileMode.Create, FileAccess.ReadWrite);
                try
                {
                    fs.SetLength(Size);
                    FileMemory fm = new FileMemory(Path);
                    fm._FileStream = fs;
                    return _Files[Path] = fm;
                }
                finally
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Insures the associated file is open and ready for reading and writing. Returns 
        /// wether the file is (or was) open.
        /// </summary>
        public bool Open()
        {
            if (this._FileStream == null)
            {
                try
                {
                    this._FileStream = System.IO.File.Open(this._Path, FileMode.Open, FileAccess.ReadWrite);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tries closing the associated file, releasing filesystem resources. The file will be reopened when needed. Returns 
        /// wether the file is (or was) closed.
        /// </summary>
        public bool Close()
        {
            if (this._Users > 0)
            {
                return false;
            }
            else
            {
                if (this._FileStream != null)
                {
                    this._FileStream.Close();
                    this._FileStream.Dispose();
                    this._FileStream = null;
                }
                return true;
            }
        }

        public override InStream Read()
        {
            return this.Open() ? new _InStream(0, this) : null;
        }

        public override OutStream Modify(long Start)
        {
            return this.Open() ? new _OutStream(Start, this) : null;
        }

        public override long Size
        {
            get
            {
                return this.Open() ? this._FileStream.Length : 0;
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
                this._File._Users++;
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

            public override void Finish()
            {
                this._File._Users--;
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
                this._File._Users++;
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

            public override void Finish()
            {
                this._File._Users--;
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

        private static Dictionary<string, FileMemory> _Files = new Dictionary<string, FileMemory>();

        private int _Users;
        private Path _Path;
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