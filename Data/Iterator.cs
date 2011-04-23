using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An interface to a collection of items that may be accessed in some sequential order.
    /// </summary>
    public interface IIterator<T>
    {
        /// <summary>
        /// Tries getting the next item from the iterator, or returns false if there are no remaining items.
        /// </summary>
        bool Get(ref T Item);

        /// <summary>
        /// Called when the iterator will no longer be used. Usually, the collection may not be modified if
        /// there is an active (unfinished) iterator for it.
        /// </summary>
        void Finish();
    }

    /// <summary>
    /// Contains functions for the manipulation of iterators.
    /// </summary>
    public static class Iterator
    {

    }
}