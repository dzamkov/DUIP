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
}