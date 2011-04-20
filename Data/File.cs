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
        public FileData Open()
        {
            try
            {
                return new FileData(this);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a file with this path. The file will be able to be read and modified.
        /// </summary>
        public FileData Create(long Size)
        {
            try
            {
                FileStream fs = File.Open(this, FileMode.Create, FileAccess.ReadWrite);
                fs.SetLength(Size);
                return new FileData(this, fs);
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
        public FileData(Path Path)
        {
            this._Path = Path;
        }

        public FileData(Path Path, FileStream FileStream)
        {
            this._Path = Path;
            this._FileStream = FileStream;
            _CloseFiles(_MaxOpenFiles);
            _OpenFiles[this._Path] = this;
        }

        /// <summary>
        /// Insures the file is available for reading or writing. This does not need to be called manually. Returns true
        /// if any action is taken.
        /// </summary>
        public bool Open()
        {
            if (this._FileStream == null)
            {
                _CloseFiles(_MaxOpenFiles);
                this._FileStream = File.Open(this._Path, FileMode.Open, FileAccess.ReadWrite);
                _OpenFiles[this._Path] = this;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tires closing the native file stream associated with the file. The file will be reopened when needed. Returns true
        /// if any action is taken.
        /// </summary>
        public bool Close()
        {
            if (this._FileStream != null)
            {
                if (!this._WriteStream && this._ReadStreams == 0)
                {
                    _OpenFiles.Remove(this._Path);
                    this._FileStream.Close();
                    this._FileStream = null;
                    return true;
                }
            }
            return false;
        }

        public override InStream Read()
        {
            this.Open();
            if (this._WriteStream)
            {
                return null;
            }
            else
            {
                return new _InStream(0, this);
            }
        }

        public override OutStream Modify(long Start)
        {
            this.Open();
            if (this._ReadStreams > 0)
            {
                return null;
            }
            else
            {
                this._FileStream.Position = Start;
                return new _OutStream(this);
            }
        }

        public override long Length
        {
            get
            {
                this.Open();
                return this._FileStream.Length;
            }
        }

        /// <summary>
        /// An input stream for file data.
        /// </summary>
        private class _InStream : InStream
        {
            public _InStream(long Position, FileData File)
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

            public override void Advance(long Amount)
            {
                this._Position += Amount;
            }

            public override long BytesAvailable
            {
                get
                {
                    return this._File._FileStream.Length - this._Position;
                }
            }

            public override void Read(byte[] Buffer, int Offset, int Length)
            {
                this._File._Seek(this._Position);
                this._Position += (long)Length;
                this._File._FileStream.Read(Buffer, Offset, Length);
            }

            public override void Finish()
            {
                this._File._ReadStreams--;
            }

            private long _Position;
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
        private void _Seek(long Position)
        {
            if (this._FileStream.Position != Position)
            {
                this._FileStream.Position = Position;
            }
        }

        public override bool Immutable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Closes files so that the amount of open files is under the given maximum.
        /// </summary>
        private static void _CloseFiles(int Max)
        {
            bool m = true;
            while (m && _OpenFiles.Count > Max)
            {
                m = false;
                foreach (var kvp in _OpenFiles)
                {
                    if (kvp.Value.Close())
                    {
                        m = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of files to keep open at any time.
        /// </summary>
        public static int MaxOpenFiles
        {
            get
            {
                return _MaxOpenFiles;
            }
            set
            {
                _MaxOpenFiles = value;
                _CloseFiles(_MaxOpenFiles);
            }
        }

        private static int _MaxOpenFiles = 20;
        private static Dictionary<string, FileData> _OpenFiles = new Dictionary<string, FileData>();

        private int _ReadStreams;
        private bool _WriteStream;
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

        public override long BytesAvailable
        {
            get
            {
                return this._FileStream.Length - this._FileStream.Position;
            }
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

        public override void Finish()
        {
            this._FileStream.Close();
        }

        private FileStream _FileStream;
    }
}