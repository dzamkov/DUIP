using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DUIP
{
    /// <summary>
    /// Represents a computational task that gives a result upon execution.
    /// </summary>
    public abstract class Task<T>
    {
        /// <summary>
        /// Synchronously executes the task and returns its result.
        /// </summary>
        public abstract T Execute();

        public static implicit operator Task<T>(T Result)
        {
            return new StaticTask<T>(Result);
        }

        public static implicit operator Task<T>(Func<T> Delegate)
        {
            return new DelegateTask<T>(Delegate);
        }
    }

    /// <summary>
    /// A task that gives a static pre-determined result upon execution.
    /// </summary>
    public sealed class StaticTask<T> : Task<T>
    {
        public StaticTask(T Result)
        {
            this._Result = Result;
        }

        /// <summary>
        /// Gets the result of this task.
        /// </summary>
        public T Result
        {
            get
            {
                return this._Result;
            }
        }

        public override T Execute()
        {
            return this._Result;
        }

        private T _Result;
    }

    /// <summary>
    /// A task that invokes a delegate when executed.
    /// </summary>
    public sealed class DelegateTask<T> : Task<T>
    {
        public DelegateTask(Func<T> Delegate)
        {
            this._Delegate = Delegate;
        }

        /// <summary>
        /// Gets the delegate for this task.
        /// </summary>
        public Func<T> Delegate
        {
            get
            {
                return this._Delegate;
            }
        }

        public override T Execute()
        {
            return this._Delegate();
        }

        private Func<T> _Delegate;
    }
}