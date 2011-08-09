using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A substitution rule.
    /// </summary>
    public abstract class Rule
    {
        public Rule(Pattern Pattern)
        {
            this.Pattern = Pattern;
        }

        /// <summary>
        /// The pattern of expressions to which this rule may be applied.
        /// </summary>
        public readonly Pattern Pattern;

        /// <summary>
        /// Gets the result of a substitution, or null, if the rule may not be applied to the instance.
        /// </summary>
        /// <param name="Scope">The scope in which the rule is applied.</param>
        /// <param name="Terms">The values for the terms of the pattern. Terms that are null have not been 
        /// defined in the pattern and may represent any value.</param>
        public abstract Task<Expression> GetResult(Scope Scope, Expression[] Terms);
    }

    /// <summary>
    /// A rule with a result defined by an expression.
    /// </summary>
    public sealed class SimpleRule : Rule
    {
        public SimpleRule(Pattern Pattern, Expression Result)
            : base(Pattern)
        {
            this.Result = Result;
        }

        public SimpleRule(int Terms, Expression Template, Expression Condition, Expression Result)
            : this(new Pattern(Terms, Template, Condition), Result)
        {

        }

        /// <summary>
        /// The expression that defines the result of this rule. Terms used in this expression will be replaced with their corresponding values
        /// from the pattern.
        /// </summary>
        public readonly Expression Result;

        /// <summary>
        /// Gets the inverse of this rule. This is a rule made by swapping the template and result for the rule.
        /// </summary>
        public SimpleRule Inverse
        {
            get
            {
                Pattern pattern = this.Pattern;
                return new SimpleRule(
                    new Pattern(pattern.Terms, this.Result, pattern.Condition),
                    pattern.Template);
            }
        }

        public override Task<Expression> GetResult(Scope Scope, Expression[] Terms)
        {
            return this.Result.Fill(Terms);
        }
    }

}