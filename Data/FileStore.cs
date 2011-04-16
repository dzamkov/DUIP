using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A store that uses the filesystem to store data.
    /// </summary>
    public class FileStore<T> : Store<T>
    {
        public FileStore(ISerialization<T> ReferenceSerialization, IOrdering<T> ReferenceOrdering)
        {
            this._ReferenceSerialization = ReferenceSerialization;
            this._ReferenceOrdering = ReferenceOrdering;
        }

        public override Query<Data> Lookup(T Reference)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the serialization used to serialize references.
        /// </summary>
        public ISerialization<T> ReferenceSerialization
        {
            get
            {
                return this._ReferenceSerialization;
            }
        }

        /// <summary>
        /// Gets an ordering for reference, for use in indexing.
        /// </summary>
        public IOrdering<T> ReferenceOrdering
        {
            get
            {
                return this._ReferenceOrdering;
            }
        }

        private ISerialization<T> _ReferenceSerialization;
        private IOrdering<T> _ReferenceOrdering;
    }
}