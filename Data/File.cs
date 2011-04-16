using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DUIP
{
    /// <summary>
    /// A file path on the file system.
    /// </summary>
    public struct Path
    {
        public Path(string Path)
        {
            this._Path = Path;
        }

        /// <summary>
        /// Gets the path for a file or folder in the directory for this path.
        /// </summary>
        public Path this[string File]
        {
            get
            {
                return this._Path + System.IO.Path.DirectorySeparatorChar + File;
            }
        }

        /// <summary>
        /// Gets the parent directory of this path. If this is a root directory, the same path is returned.
        /// </summary>
        public Path Parent
        {
            get
            {
                return System.IO.Path.GetDirectoryName(this) ?? this;
            }
        }

        /// <summary>
        /// Gets wether there is a file or directory at this path.
        /// </summary>
        public bool Exists
        {
            get
            {
                return Directory.Exists(this) || File.Exists(this);
            }
        }

        /// <summary>
        /// Gets wether there is a directory at this path.
        /// </summary>
        public bool DirectoryExists
        {
            get
            {
                return Directory.Exists(this);
            }
        }

        /// <summary>
        /// Gets wether there is a file at this path.
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(this);
            }
        }

        /// <summary>
        /// Insures a directory exists at this path. Returns true if a new directory was created.
        /// </summary>
        public bool MakeDirectory()
        {
            if (this.DirectoryExists)
            {
                return false;
            }
            if (this.FileExists)
            {
                this.Delete();
            }
            this.Parent.MakeDirectory();
            Directory.CreateDirectory(this);
            return true;
        }

        /// <summary>
        /// Deletes whatever is at this path if anything. Returns false if nothing exists at this path.
        /// </summary>
        public bool Delete()
        {
            if (this.FileExists)
            {
                File.Delete(this);
                return true;
            }
            if (this.DirectoryExists)
            {
                Directory.Delete(this);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets or sets the path for the working directory.
        /// </summary>
        public static Path WorkingDirectory
        {
            get
            {
                return Environment.CurrentDirectory;
            }
            set
            {
                Environment.CurrentDirectory = value;
            }
        }

        /// <summary>
        /// Opens the file at this path as data. If no file exists, null will be returned.
        /// </summary>
        public FileData Open(bool AllowRead, bool AllowWrite)
        {
            try
            {
                return new FileData(File.Open(this, FileMode.Open, _GetAccess(AllowRead, AllowWrite)));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a file with this path. The file will be able to be read and modified.
        /// </summary>
        public FileData Create(int Size)
        {
            try
            {
                FileStream fs = File.Open(this, FileMode.Create, FileAccess.ReadWrite);
                fs.SetLength((long)Size);
                return new FileData(fs);
            }
            catch
            {
                return null;
            }
        }

        private static FileAccess _GetAccess(bool Read, bool Write)
        {
            if (Read)
            {
                if (Write)
                {
                    return FileAccess.ReadWrite;
                }
                return FileAccess.Read;
            }
            return FileAccess.Write;
        }

        /// <summary>
        /// Creates a stream to read the file.
        /// </summary>
        public FileInStream OpenRead()
        {
            try
            {
                return new FileInStream(File.OpenRead(this));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a stream to write to or over a file. If the file already exists, its original contents will be lost.
        /// </summary>
        public FileOutStream OpenWrite()
        {
            try
            {
                return new FileOutStream(File.Open(this, FileMode.Create, FileAccess.Write));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a stream to append data to a file.
        /// </summary>
        public FileOutStream OpenAppend()
        {
            try
            {
                return new FileOutStream(File.Open(this, FileMode.Append, FileAccess.Write));
            }
            catch
            {
                return null;
            }
        }

        public static implicit operator string(Path Path)
        {
            return Path._Path;
        }

        public static implicit operator Path(string Path)
        {
            return new Path(Path);
        }

        private string _Path;
    }

    /// <summary>
    /// Data from a file on the filesystem. Note that file data should be closed manually when no longer needed.
    /// </summary>
    public class FileData : Data
    {
        public FileData(FileStream Stream)
        {
            this._FileStream = Stream;
        }

        /// <summary>
        /// Closes the data, releases resources required for reading the file and preventing further use of this data. This must also be
        /// called before writing to the file.
        /// </summary>
        public void Close()
        {
            this._FileStream.Close();
        }

        public override InStream Read()
        {
            if (this._WriteStream || !this._FileStream.CanRead)
            {
                return null;
            }
            else
            {
                return new _InStream(0, this);
            }
        }

        public override OutStream Modify(int Start)
        {
            if (this._ReadStreams > 0 || !this._FileStream.CanWrite)
            {
                return null;
            }
            else
            {
                this._FileStream.Position = (long)Start;
                return new _OutStream(this);
            }
        }

        public override int Length
        {
            get
            {
                return (int)this._FileStream.Length;
            }
        }

        /// <summary>
        /// An input stream for file data.
        /// </summary>
        private class _InStream : InStream
        {
            public _InStream(int Position, FileData File)
            {
                this._Position = Position;
                this._File = File;
                this._File._ReadStreams++;
            }

            public override byte Read()
            {
                this._File._Seek(this._Position);
                this._Position++;
                return (byte)this._File._FileStream.ReadByte();
            }

            public override void Advance(int Amount)
            {
                this._Position += Amount;
            }

            public override int BytesAvailable
            {
                get
                {
                    return (int)this._File._FileStream.Length - this._Position;
                }
            }

            public override void Read(byte[] Buffer, int Offset, int Length)
            {
                this._File._Seek(this._Position);
                this._Position += Length;
                this._File._FileStream.Read(Buffer, Offset, Length);
            }

            public override void Finish()
            {
                this._File._ReadStreams--;
            }
                    
            private int _Position;
            private FileData _File;
        }

        /// <summary>
        /// An output stream for file data.
        /// </summary>
        private class _OutStream : OutStream
        {
            public _OutStream(FileData File)
            {
                this._File = File;
                this._File._WriteStream = true;
            }

            public override void Write(byte Data)
            {
                this._File._FileStream.WriteByte(Data);
            }

            public override void Write(byte[] Buffer, int Offset, int Length)
            {
                this._File._FileStream.Write(Buffer, Offset, Length);
            }

            public override void Finish()
            {
                this._File._WriteStream = false;
            }

            private FileData _File;
        }

        /// <summary>
        /// Insures the filestream is at the given position in the file.
        /// </summary>
        private void _Seek(int Position)
        {
            if ((int)this._FileStream.Position != Position)
            {
                this._FileStream.Position = (long)Position;
            }
        }

        public override bool Immutable
        {
            get
            {
                return !this._FileStream.CanWrite;
            }
        }

        private int _ReadStreams;
        private bool _WriteStream;
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

        public override int BytesAvailable
        {
            get
            {
                return (int)(this._FileStream.Length - this._FileStream.Position);
            }
        }

        public override void Advance(int Amount)
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

        public override void Finish()
        {
            this._FileStream.Close();
        }

        private FileStream _FileStream;
    }
}