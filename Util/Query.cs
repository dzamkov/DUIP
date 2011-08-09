using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DUIP
{
    /// <summary>
    /// Represents a process that will given a result upon completion. Once the query is complete,
    /// it will retain the result.
    /// </summary>
    public abstract class Query<T>
    {
        /// <summary>
        /// Registers a listener for the query. Once a result for the query is available, the listener will be called and then removed. The listener
        /// can be manually removed by calling the returned remove handler.
        /// </summary>
        public abstract RemoveHandler Register(Action<T> Listener);

        /// <summary>
        /// Gets the result of the query, if available.
        /// </summary>
        public abstract Maybe<T> Result { get; }

        public static implicit operator Query<T>(T Value)
        {
            return new StaticQuery<T>(Value);
        }
    }

    /// <summary>
    /// A query that returns a value without performing any operations.
    /// </summary>
    public class StaticQuery<T> : Query<T>
    {
        public StaticQuery(T Value)
        {
            this.Value = Value;
        }

        /// <summary>
        /// The value of this query.
        /// </summary>
        public readonly T Value;

        public override RemoveHandler Register(Action<T> Listener)
        {
            Listener(this.Value);
            return null;
        }

        public override Maybe<T> Result
        {
            get
            {
                return this.Value;
            }
        }
    }

    /// <summary>
    /// A query whose result is manually given some time after the query is created.
    /// </summary>
    public class DelayedQuery<T> : Query<T>
    {
        /// <summary>
        /// Indicates that the query is complete and provides a result for it.
        /// </summary>
        public void Complete(T Result)
        {
            this._Result = Result;
            if (this._Listeners != null)
            {
                this._Listeners(Result);
                this._Listeners = null;
            }
        }

        /// <summary>
        /// Indicates that the query will never be complete.
        /// </summary>
        public void Cancel()
        {
            this._Listeners = null;
        }

        public override RemoveHandler Register(Action<T> Listener)
        {
            if (this._Result.HasValue)
            {
                Listener(this._Result.Value);
                return null;
            }
            else
            {
                this._Listeners += Listener;
                return delegate { this._Listeners -= Listener; };
            }
        }

        public override Maybe<T> Result
        {
            get
            {
                return this._Result;
            }
        }

        private Maybe<T> _Result;
        private Action<T> _Listeners;
    }
}