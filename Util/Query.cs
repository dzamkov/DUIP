using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DUIP
{
    /// <summary>
    /// Represents a process that will output a result upon completion. Once the query is complete,
    /// it will retain its result.
    /// </summary>
    public abstract class Query<T>
    {
        /// <summary>
        /// Registers a listener for the query. When the query is complete, the listener will
        /// be called with the result of the query. If the query is already complete, the listener
        /// is called immediately.
        /// </summary>
        public abstract void Register(Action<T> Listener);

        public static implicit operator Query<T>(T Value)
        {
            return new StaticQuery<T>(Value);
        }

        public static implicit operator Query<T>(Func<T> Function)
        {
            return new ComputationQuery<T>(Function);
        }
    }

    /// <summary>
    /// A query that returns a value without performing any operations.
    /// </summary>
    public class StaticQuery<T> : Query<T>
    {
        public StaticQuery(T Value)
        {
            this._Value = Value;
        }

        /// <summary>
        /// Gets the value of this query.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        public override void Register(Action<T> Listener)
        {
            Listener(this._Value);
        }

        private T _Value;
    }

    /// <summary>
    /// A query that combines the results of several other queries.
    /// </summary>
    public sealed class CompoundQuery<T> : Query<T>
    {
        public CompoundQuery()
        {
            this._Listeners = new List<Action<T>>();
        }

        /// <summary>
        /// Requires a query to be complete in order for this query to be computed.
        /// </summary>
        /// <param name="Result">A reference that is modified upon completion of the given query to reflect its result.</param>
        public void Require<F>(Query<F> Query, Ref<F> Result)
        {
            this._Required++;
            Query.Register(delegate(F Value)
            {
                Result.Value = Value;
                if (--this._Required == 0 && this._Function != null)
                {
                    this._Evaluate();
                }
            });
        }

        public override void Register(Action<T> Listener)
        {
            if (this._Listeners == null)
            {
                this._Result.Register(Listener);
            }
            else
            {
                this._Listeners.Add(Listener);
            }
        }

        /// <summary>
        /// Gets or sets the function used to determine what to do after after all required queries are completed.
        /// </summary>
        public Func<Query<T>> Function
        {
            get
            {
                return this._Function;
            }
            set
            {
                this._Function = value;
                if (this._Required == 0)
                {
                    this._Evaluate();
                }
            }
        }

        /// <summary>
        /// Evaluates the function for the compound query.
        /// </summary>
        private void _Evaluate()
        {
            this._Result = this._Function();
            this._Function = null;
            foreach (Action<T> listener in this._Listeners)
            {
                this._Result.Register(listener);
            }
            this._Listeners = null;
        }

        private int _Required;
        private Query<T> _Result;
        private Func<Query<T>> _Function;
        private List<Action<T>> _Listeners;
    }

    /// <summary>
    /// A query that asynchronously evaluates a function.
    /// </summary>
    public sealed class ComputationQuery<T> : Query<T>
    {
        public ComputationQuery(Func<T> Function)
        {
            this._Listeners = new List<Action<T>>();
            this._Thread = new Thread(delegate()
            { 
                T result = this._Result = Function();
                lock (this)
                {
                    foreach (Action<T> listener in this._Listeners)
                    {
                        listener(result);
                    }
                    this._Listeners = null;
                    this._Thread = null;
                }
            });
            this._Thread.IsBackground = true;
            this._Thread.Start();
        }

        public override void Register(Action<T> Listener)
        {
            lock (this)
            {
                if (this._Listeners == null)
                {
                    Listener(this._Result);
                }
                else
                {
                    this._Listeners.Add(Listener);
                }
            }
        }

        /// <summary>
        /// Blocks the current thread until the result for this query is known.
        /// </summary>
        public T Wait()
        {
            if (this._Thread != null)
            {
                this._Thread.Join();
            }
            return this._Result;
        }

        private T _Result;
        private Thread _Thread;
        private List<Action<T>> _Listeners;
    }
}