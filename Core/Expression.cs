using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Represents a computional relationship between values and untyped variables.
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        /// Gets the free variables referenced in this expression.
        /// </summary>
        public IEnumerable<int> Variables
        {
            get
            {
                HashSet<int> vars = new HashSet<int>();
                this.OutputVariables(vars);
                return vars;
            }
        }

        /// <summary>
        /// Adds the free variables referenced in this expression to the given set without otherwise altering
        /// the set's contents.
        /// </summary>
        public virtual void OutputVariables(HashSet<int> Variables)
        {

        }

        /// <summary>
        /// Substitutes the variables in this expression based on the given map. If a variable does not appear in the map,
        /// it should remain unchanged.
        /// </summary>
        public virtual Expression Substitute(Dictionary<int, Expression> Map)
        {
            return this;
        }

        /// <summary>
        /// A type/value expression pair.
        /// </summary>
        public struct Pair
        {
            public Pair(Expression Type, Expression Value)
            {
                this.Type = Type;
                this.Value = Value;
            }

            /// <summary>
            /// The type component of this expression pair.
            /// </summary>
            public Expression Type;

            /// <summary>
            /// The value component of this expression pair.
            /// </summary>
            public Expression Value;
        }

        /// <summary>
        /// Checks wether this expression is consistent when using the given variable types and values (which may be interrelated or
        /// recursively defined). Returns a typed, optimized form of this expression that contains no implicit conversions.
        /// </summary>
        public abstract bool Check(Dictionary<int, Pair> Variables, ref Pair Result);

        /// <summary>
        /// Creates a function that evaluates this expression with variable 0 as the argument. Returns null if
        /// the expression (when interpreted with the given argument type) is invalid.
        /// </summary>
        public Function Build(Type Argument, out Type Result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an expression for a constant value.
        /// </summary>
        public static ConstantExpression Constant(Type Type, object Value)
        {
            return new ConstantExpression(Type, Value);
        }

        /// <summary>
        /// Gets an expression for a variable.
        /// </summary>
        public static VariableExpression Variable(int Index)
        {
            return new VariableExpression(Index);
        }
    }

    /// <summary>
    /// An expression representation of a constant value.
    /// </summary>
    public sealed class ConstantExpression : Expression
    {
        public ConstantExpression(Type Type, object Value)
        {
            this._Type = Type;
            this._Value = Value;
        }

        /// <summary>
        /// Gets the type of the constant.
        /// </summary>
        public Type Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        public object Value
        {
            get
            {
                return this._Value;
            }
        }

        public override bool Check(Dictionary<int, Pair> Variables, ref Pair Result)
        {
            Result = new Pair(Constant(Type.Reflexive, this._Type), this);
            return true;
        }

        private Type _Type;
        private object _Value;
    }

    /// <summary>
    /// An expression that takes the value of an indexed variable.
    /// </summary>
    public sealed class VariableExpression : Expression
    {
        public VariableExpression(int Index)
        {
            this._Index = Index;
        }

        /// <summary>
        /// The index of the variable this expression uses.
        /// </summary>
        public int Index
        {
            get
            {
                return this._Index;
            }
        }

        public override void OutputVariables(HashSet<int> Variables)
        {
            Variables.Add(this._Index);
        }

        public override Expression Substitute(Dictionary<int, Expression> Map)
        {
            Expression res;
            if (Map.TryGetValue(this._Index, out res))
            {
                return res;
            }
            return this;
        }

        public override bool Check(Dictionary<int, Expression.Pair> Variables, ref Expression.Pair Result)
        {
            Result = Variables[this._Index];
            return true;
        }

        private int _Index;
    }
}