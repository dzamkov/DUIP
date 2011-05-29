﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents a static, named container of information that can be imported from or exported to a filesystem.
    /// </summary>
    public sealed class File
    {
        private File()
        {

        }

        /// <summary>
        /// Creates a file with the given data.
        /// </summary>
        public static File CreateDataFile(string Name, Data Data)
        {
            return new File
            {
                _Name = Name,
                _Data = Data
            };
        }

        /// <summary>
        /// Creates a folder with the given subfiles.
        /// </summary>
        public static File CreateFolder(string Name, IEnumerable<File> Files)
        {
            return new File
            {
                _Name = Name,
                _Subfiles = 
                        (from File f in Files
                        orderby f.Name ascending
                        select f).ToArray() // Order the files by name for faster search and folder comparision
            };
        }

        /// <summary>
        /// Gets if the given files have equivalent names and contents.
        /// </summary>
        public static bool Equal(File A, File B)
        {
            return A.Name == B.Name && EqualContent(A, B);
        }

        /// <summary>
        /// Gets if the given files have equivalent contents.
        /// </summary>
        public static bool EqualContent(File A, File B)
        {
            if (A._Subfiles != null)
            {
                if (B._Subfiles != null)
                {
                    if (A._Subfiles.Length != B._Subfiles.Length)
                    {
                        return false;
                    }
                    for (int t = 0; t < A._Subfiles.Length; t++)
                    {
                        // Compare names of subfiles first to put off costly content checking
                        if (A._Subfiles[t].Name != B._Subfiles[t].Name)
                        {
                            return false;
                        }
                    }
                    for (int t = 0; t < A._Subfiles.Length; t++)
                    {
                        if (!EqualContent(A._Subfiles[t], B._Subfiles[t]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            if (A._Data != null)
            {
                if (B._Data != null)
                {
                    return Data.Equal(A._Data, B._Data);
                }
                return false;
            }

            // Hopefully, this will never be happen
            return false;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        /// <summary>
        /// Gets wether this file is a folder (contains other files).
        /// </summary>
        public bool Folder
        {
            get
            {
                return this._Subfiles != null;
            }
        }

        private string _Name;
        private Data _Data;
        private File[] _Subfiles;
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
            return DUIP.File.Equal(A, B);
        }

        public override UI.Block CreateBlock(UI.Theme Theme, File Instance)
        {
            return Theme.GetTextBlock(Instance.Name);
        }
    }
}