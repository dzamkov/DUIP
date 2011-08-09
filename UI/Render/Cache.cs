using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.Memory;

namespace DUIP.UI.Render
{
    /// <summary>
    /// An interface to a cache that allows indexed rendering data to be stored between program runs.
    /// </summary>
    public interface IRenderCache
    {
        /// <summary>
        /// Gets a stream to update (or create) the data for the item with the given name. Returns null if not possible.
        /// </summary>
        Disposable<OutStream> Update(string Name);

        /// <summary>
        /// Gets a stream to read the data for the item with the given name. Returns null if not possible, or if the item does not exist.
        /// </summary>
        Disposable<InStream> Read(string Name);
    }

    /// <summary>
    /// A render cache that stores items on the filesystem.
    /// </summary>
    public class FileSystemRenderCache : IRenderCache
    {
        public FileSystemRenderCache(Path Path)
        {
            this.Path = Path;
            this.Path.MakeDirectory();
        }

        /// <summary>
        /// The path for the directory to store cached items to.
        /// </summary>
        public readonly Path Path;

        public Disposable<OutStream> Update(string Name)
        {
            Path path = this.Path[Name];
            return (FileOutStream)path.OpenWrite();
        }

        public Disposable<InStream> Read(string Name)
        {
            Path path = this.Path[Name];
            if (path.FileExists)
            {
                return (FileInStream)path.OpenRead();
            }
            else
            {
                return null;
            }
        }
    }
}