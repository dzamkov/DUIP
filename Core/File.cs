using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents a static, named container of information that can be imported from or exported to a filesystem.
    /// </summary>
    public abstract class File
    {
        /// <summary>
        /// Gets the name of this file.
        /// </summary>
        public abstract string Name { get; }
    }

    /// <summary>
    /// A file that contains a collection of related files.
    /// </summary>
    public abstract class FolderFile : File
    {
        /// <summary>
        /// Gets the file in this folder with the given name, or null if no such file exists.
        /// </summary>
        public virtual File this[string Name]
        {
            get
            {
                foreach(File f in this.Files)
                {
                    if(f.Name == Name)
                    {
                        return f;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the files in this folder.
        /// </summary>
        public abstract IEnumerable<File> Files { get; }
    }

    /// <summary>
    /// A file that contains uninterpreted data.
    /// </summary>
    public abstract class DataFile : File
    {
        /// <summary>
        /// Gets the data for this file.
        /// </summary>
        public abstract Data Data { get; }
    }

    /// <summary>
    /// A type for a file.
    /// </summary>
    public class FileType : Type<File>
    {
        private FileType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly FileType Singleton = new FileType();

        public sealed override bool Equal(File A, File B)
        {
            // Compare names
            if (A.Name != B.Name)
            {
                return false;
            }

            // Compare as folders
            FolderFile ffa = A as FolderFile;
            if (ffa != null)
            {
                FolderFile ffb = B as FolderFile;
                if (ffb != null)
                {
                    foreach (File asubfile in ffa.Files)
                    {
                        File bsubfile = ffb[asubfile.Name];
                        if (bsubfile == null || !this.Equal(asubfile, bsubfile))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            // Compare as data files
            DataFile dfa = A as DataFile;
            if (dfa != null)
            {
                DataFile dfb = B as DataFile;
                if (dfb != null)
                {
                    return Type.Data.Equal(dfa.Data, dfb.Data);
                }
                return false;
            }

            // Safe answer
            return false;
        }
    }
}