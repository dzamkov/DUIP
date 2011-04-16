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
        /// Opens the file at this path as data.
        /// </summary>
        public FileData Open
        {
            get
            {
                try
                {
                    return new FileData(File.OpenRead(this));
                }
                catch
                {
                    return null;
                }
            }
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
        /// Creates a stream to write to or over a file.
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

        public override InStream Read
        {
            get
            {
                throw new NotImplementedException();
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
        /// A stream for file data.
        /// </summary>
        private class _Stream : InStream
        {
            public _Stream(int Position, FileStream FileStream)
            {
                this._Position = Position;
                this._FileStream = FileStream;
            }

            public override byte Read()
            {
                this._Scan();
                return (byte)this._FileStream.ReadByte();
            }

            public override void Advance(int Amount)
            {
                this._Position += Amount;
            }

            /// <summary>
            /// Goes to the correct position in the file.
            /// </summary>
            private void _Scan()
            {
                if ((int)this._FileStream.Position != this._Position)
                {
                    this._FileStream.Position = (long)this._Position;
                }
            }

            public override int BytesAvailable
            {
                get
                {
                    return (int)this._FileStream.Length - this._Position;
                }
            }

            public override void Read(byte[] Buffer, int Offset, int Length)
            {
                this._Scan();
                this._FileStream.Read(Buffer, Offset, Length);
            }
                    
            private int _Position;
            private FileStream _FileStream;
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