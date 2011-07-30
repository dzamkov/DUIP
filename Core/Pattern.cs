using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A class of expressions that have a certain structure (given by a template) and meet certain conditions.
    /// </summary>
    public sealed class Pattern
    {
        public Pattern(int Terms, Expression Template, Expression Condition)
        {
            this._Terms = Terms;
            this._Template = Template;
            this._Condition = Condition;
        }

        /// <summary>
        /// Gets the amount of unique terms used in this pattern. Terms may represent any more specialized expression, by must
        /// be consistent in an instance of the pattern.
        /// </summary>
        public int Terms
        {
            get
            {
                return this._Terms;
            }
        }

        /// <summary>
        /// Gets the template for the pattern. The template should make use of terms which can take the place of any other expression. All instance of this
        /// pattern must have a structure that can be described by this template with a varying configuration of sub-expressions in place of the terms.
        /// </summary>
        public Expression Template
        {
            get
            {
                return this._Template;
            }
        }

        /// <summary>
        /// Gets the condition for the pattern. In order for an expression to be an instance of this pattern, this expression must evaluate to true with the
        /// established terms substituted in. It is possible for some terms for a pattern to be used in the condition but not in the template; in which case, these
        /// terms may take any expression to make the condition true.
        /// </summary>
        public Expression Condition
        {
            get
            {
                return this._Condition;
            }
        }

        private int _Terms;
        private Expression _Template;
        private Expression _Condition;
    }

    /// <summary>
    /// A placeholder for a sub-expression in a pattern. Note that this is not an actual expression, and should only be used in
    /// patterns or expressions dependant on them.
    /// </summary>
    public sealed class Term : Expression
    {
        public Term(int Index)
        {
            this._Index = Index;
        }

        /// <summary>
        /// Gets the index of this term in the pattern.
        /// </summary>
        public int Index
        {
            get
            {
                return this._Index;
            }
        }

        public override Expression Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private int _Index;
    }
}