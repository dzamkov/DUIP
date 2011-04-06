using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// An executable command that outputs a result of a certain type when complete.
    /// </summary>
    public abstract class Query<T>
    {
        /// <summary>
        /// Synchronously executes the query and gets its result.
        /// </summary>
        public abstract T Execute();

        public static implicit operator Query<T>(T Value)
        {
            return new StaticQuery<T>(Value);
        }

        public static implicit operator T(Query<T> Query)
        {
            return Query.Execute();
        }

        /// <summary>
        /// Creates a query based on a function.
        /// </summary>
        public static FunctionQuery<T> Create(Func<T> Function)
        {
            return new FunctionQuery<T>(Function);
        }

        /// <summary>
        /// Creates a query based on the result of this query.
        /// </summary>
        public virtual Query<F> Bind<F>(Func<T, Query<F>> Map)
        {
            return new ChainQuery<T, F>(this, Map);
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

        public override T Execute()
        {
            return this._Value;
        }

        public override Query<F> Bind<F>(Func<T, Query<F>> Map)
        {
            return Map(this._Value);
        }

        private T _Value;
    }

    /// <summary>
    /// A query that is defined by a function.
    /// </summary>
    public class FunctionQuery<T> : Query<T>
    {
        public FunctionQuery(Func<T> Function)
        {
            this._Function = Function;
        }

        public override T Execute()
        {
            return this._Function();
        }

        private Func<T> _Function;
    }

    /// <summary>
    /// A query created by chaining together a query and a function based on its result.
    /// </summary
    public class ChainQuery<T, F> : Query<F>
    {
        public ChainQuery(Query<T> Query, Func<T, Query<F>> Function)
        {
            this._Query = Query;
            this._Function = Function;
        }

        /// <summary>
        /// Gets the original query that must be performed first.
        /// </summary>
        public Query<T> Query
        {
            get
            {
                return this._Query;
            }
        }

        /// <summary>
        /// Gets the function to be called once the first query is complete.
        /// </summary>
        public Func<T, Query<F>> Function
        {
            get
            {
                return this._Function;
            }
        }

        public override F Execute()
        {
            return this._Function(this._Query.Execute()).Execute();
        }

        private Query<T> _Query;
        private Func<T, Query<F>> _Function;
    }
}