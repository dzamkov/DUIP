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
        /// Registers a listener for the query.
        /// </summary>
        /// <remarks>At most one method will be called on the listener. After a method is called, the listener will no
        /// longer be tracked by the query.</remarks>
        public abstract void Register(IQueryListener<T> Listener);

        /// <summary>
        /// Registers a listener (given by a delegate to be invoked upon completion) for the query.
        /// </summary>
        public void Register(Action<T> Listener)
        {
            this.Register(new DelegateQueryListener<T>(Listener));
        }

        /// <summary>
        /// Gets the result of the query, if available.
        /// </summary>
        public abstract Maybe<T> Result { get; }

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
    /// An interface that responds to the completion (or cancellation) of a query.
    /// </summary>
    /// <remarks>The methods in a listener may be called with any thread.</remarks>
    public interface IQueryListener<T>
    {
        /// <summary>
        /// Called when the query is complete with the result of the query.
        /// </summary>
        void Complete(T Result);

        /// <summary>
        /// Called when the query is cancelled, indicating that the query will never be complete.
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// A query listener that invokes a delegate upon completion.
    /// </summary>
    public class DelegateQueryListener<T> : IQueryListener<T>
    {
        public DelegateQueryListener(Action<T> Action)
        {
            this._Action = Action;
        }

        /// <summary>
        /// Gets the action this listener invokes when the query is complete.
        /// </summary>
        public Action<T> Action
        {
            get
            {
                return this._Action;
            }
        }

        public void Complete(T Result)
        {
            this._Action(Result);
        }

        public void Cancel()
        {

        }

        private Action<T> _Action;
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

        public override void Register(IQueryListener<T> Listener)
        {
            Listener.Complete(this._Value);
        }

        public override Maybe<T> Result
        {
            get
            {
                return this._Value;
            }
        }

        private T _Value;
    }

    /// <summary>
    /// A query that will never be complete in any case (immediately calls cancel on all registered listeners).
    /// </summary>
    public class NeverQuery<T> : Query<T>
    {
        private NeverQuery()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static NeverQuery<T> Instance = new NeverQuery<T>();

        public override void Register(IQueryListener<T> Listener)
        {
            Listener.Cancel();
        }

        public override Maybe<T> Result
        {
            get
            {
                return Maybe<T>.Nothing;
            }
        }
    }

    /// <summary>
    /// A query that depends on the results of several other queries.
    /// </summary>
    public sealed class CompoundQuery<T> : Query<T>
    {
        public CompoundQuery()
        {
            this._Listeners = new List<IQueryListener<T>>();
        }

        /// <summary>
        /// Requires a query to be complete in order for this query to be computed.
        /// </summary>
        /// <param name="Result">A reference that is modified upon completion of the given query to reflect its result.</param>
        public void Require<F>(Query<F> Query, Ref<F> Result)
        {
            this._Required++;
            Query.Register(new _Listener<F>
            {
                Main = this,
                Result = Result
            });
        }

        /// <summary>
        /// A listener for a required query.
        /// </summary>
        private class _Listener<F> : IQueryListener<F>
        {
            public void Complete(F Result)
            {
                this.Result.Value = Result;
                if (--this.Main._Required == 0 && this.Main.Function != null)
                {
                    this.Main._Evaluate();
                }
            }

            public void Cancel()
            {
                this.Main._Cancel();
            }

            public CompoundQuery<T> Main;
            public Ref<F> Result;
        }

        public override void Register(IQueryListener<T> Listener)
        {
            if (this._Listeners != null)
            {
                this._Listeners.Add(Listener);
            }
            else
            {
                this._Intermediate.Register(Listener);
            }
        }

        public override Maybe<T> Result
        {
            get
            {
                if (this._Intermediate == null)
                {
                    return this._Intermediate.Result;
                }
                return Maybe<T>.Nothing;
            }
        }

        /// <summary>
        /// Gets or sets the function used to determine the intermediate query after after all required queries are completed.
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
        /// Evaluates the function for the compound query to find the intermediate query.
        /// </summary>
        private void _Evaluate()
        {
            this._Intermediate = this._Function();
            this._Function = null;
            foreach (IQueryListener<T> listener in this._Listeners)
            {
                this._Intermediate.Register(listener);
            }
            this._Listeners = null;
        }

        /// <summary>
        /// Cancels the compound query (as a result of one of the required queries being cancelled).
        /// </summary>
        private void _Cancel()
        {
            this._Intermediate = NeverQuery<T>.Instance;
            this._Function = null;
            foreach (IQueryListener<T> listener in this._Listeners)
            {
                listener.Cancel();
            }
            this._Listeners = null;
        }

        private int _Required;
        private Query<T> _Intermediate;
        private Func<Query<T>> _Function;
        private List<IQueryListener<T>> _Listeners;
    }

    /// <summary>
    /// A query that is depends on the result of a (computationally intensive) function.
    /// </summary>
    public sealed class ComputationQuery<T> : Query<T>
    {
        public ComputationQuery(Func<T> Function)
        {
            this._Listeners = new List<IQueryListener<T>>();
            this._Thread = new Thread(delegate()
            { 
                T result = this._Result = Function();
                lock (this)
                {
                    foreach (IQueryListener<T> listener in this._Listeners)
                    {
                        listener.Complete(result);
                    }
                    this._Listeners = null;
                    this._Thread = null;
                    this._HasResult = true;
                }
            });
            this._Thread.IsBackground = true;
            this._Thread.Start();
        }

        public override void Register(IQueryListener<T> Listener)
        {
            lock (this)
            {
                if (this._Listeners != null)
                {
                    this._Listeners.Add(Listener);
                }
                else
                {
                    if (this._HasResult)
                    {
                        Listener.Complete(this._Result);
                    }
                    else
                    {
                        Listener.Cancel();
                    }
                }
            }
        }

        public override Maybe<T> Result
        {
            get
            {
                return new Maybe<T>()
                {
                    HasValue = this._HasResult,
                    Value = this._Result
                };
            }
        }

        /// <summary>
        /// Blocks the current thread until the result for this query is known.
        /// </summary>
        public T Wait()
        {
            lock (this)
            {
                if (this._Thread != null)
                {
                    this._Thread.Join();
                }
            }
            return this._Result;
        }

        /// <summary>
        /// Aborts the computation, preventing it from ever being completed.
        /// </summary>
        public void Abort()
        {
            lock (this)
            {
                if (this._Thread != null)
                {

                    this._Thread.Abort();
                    this._Thread = null;
                    foreach (IQueryListener<T> listener in this._Listeners)
                    {
                        listener.Cancel();
                    }
                    this._Listeners = null;
                }
            }
        }

        private bool _HasResult;
        private T _Result;
        private Thread _Thread;
        private List<IQueryListener<T>> _Listeners;
    }

    /// <summary>
    /// A query that is depends on the result of an independant process.
    /// </summary>
    public class DelayedQuery<T> : Query<T>
    {
        public DelayedQuery()
        {
            this._Listeners = new List<IQueryListener<T>>();
        }

        public override void Register(IQueryListener<T> Listener)
        {
            if (this._Listeners != null)
            {
                this._Listeners.Add(Listener);
            }
            else
            {
                if (this._HasResult)
                {
                    Listener.Complete(this._Result);
                }
                else
                {
                    Listener.Cancel();
                }
            }
        }

        public override Maybe<T> Result
        {
            get
            {
                return new Maybe<T>()
                {
                    HasValue = this._HasResult,
                    Value = this._Result
                };
            }
        }

        /// <summary>
        /// Indicates that the process associated with the query is complete and gives the query its result. If the query has already been completed,
        /// this will have no affect.
        /// </summary>
        public void Complete(T Result)
        {
            if (!this._HasResult)
            {
                this._Result = Result;
                foreach (IQueryListener<T> listener in this._Listeners)
                {
                    listener.Complete(Result);
                }
                this._Listeners = null;
                this._HasResult = true;
            }
        }

        /// <summary>
        /// Indicates that there will never be a result for this query.
        /// </summary>
        public void Cancel()
        {
            foreach (IQueryListener<T> listener in this._Listeners)
            {
                listener.Cancel();
            }
            this._Listeners = null;
        }

        private bool _HasResult;
        private T _Result;
        private List<IQueryListener<T>> _Listeners;
    }
}