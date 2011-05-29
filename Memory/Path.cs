using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DUIP
{
    using SysPath = System.IO.Path;
    using SysFile = System.IO.File;

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
                return this._Path + SysPath.DirectorySeparatorChar + File;
            }
        }

        /// <summary>
        /// Gets the parent directory of this path. If this is a root directory, the same path is returned.
        /// </summary>
        public Path Parent
        {
            get
            {
                return SysPath.GetDirectoryName(this) ?? this;
            }
        }

        /// <summary>
        /// Gets the name of the file or folder at this path.
        /// </summary>
        public string Name
        {
            get
            {
                return SysPath.GetFileName(this);
            }
        }

        /// <summary>
        /// Gets the files (and folders) in the directory at this path.
        /// </summary>
        public IEnumerable<Path> Subfiles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets wether there is a file or directory at this path.
        /// </summary>
        public bool Exists
        {
            get
            {
                return Directory.Exists(this) || SysFile.Exists(this);
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
                return SysFile.Exists(this);
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
                SysFile.Delete(this);
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
        /// Gets a file representation of the contents at this path, or null if the path does not exist.
        /// </summary>
        public File GetFile()
        {
            if (this.DirectoryExists)
            {
                return File.CreateFolder(this.Name,
                    from file in this.Subfiles
                    select file.GetFile());
            }
            if (this.FileExists)
            {
                FileMemory fm = this.Open();
                return File.CreateDataFile(this.Name, fm.GetData(0, fm.Size));
            }
            return null;
        }

        /// <summary>
        /// Opens the file at this path as data. If no file exists, null will be returned.
        /// </summary>
        public FileMemory Open()
        {
            try
            {
                return new FileMemory(SysFile.Open(this, FileMode.Open, FileAccess.ReadWrite));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a file with this path. The file will be able to be read and modified.
        /// </summary>
        public FileMemory Create(long Size)
        {
            try
            {
                FileStream fs = SysFile.Open(this, FileMode.Create, FileAccess.ReadWrite);
                fs.SetLength(Size);
                return new FileMemory(fs);
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
                return new FileInStream(SysFile.OpenRead(this));
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
                return new FileOutStream(SysFile.Open(this, FileMode.Create, FileAccess.Write));
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
                return new FileOutStream(SysFile.Open(this, FileMode.Append, FileAccess.Write));
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
}