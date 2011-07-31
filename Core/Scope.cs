using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A container of propositions and rules that describe how expressions can be manipulated.
    /// </summary>
    public class Scope
    {
        public Scope()
        {
            this._Rules = new List<Rule>();
        }

        /// <summary>
        /// Defines a valid substitution rule for the scope.
        /// </summary>
        public void DefineRule(Rule Rule)
        {
            this._Rules.Add(Rule);
        }

        /// <summary>
        /// Defines a bidirectional rule that allows expressions to be swapped between the given forms using terms as placeholders
        /// for more specialized expressions.
        /// </summary>
        public void DefineProperty(int Terms, Expression A, Expression B)
        {
            this.DefineRule(Terms, A, B, Bool.True);
        }

        /// <summary>
        /// Defines a bidirectional rule that allows expressions to be swapped (when the condition is true) between the given forms using terms as placeholders
        /// for more specialized expressions.
        /// </summary>
        public void DefineProperty(int Terms, Expression A, Expression B, Expression Condition)
        {
            this.DefineRule(new SimpleRule(Terms, A, Condition, B));
            this.DefineRule(new SimpleRule(Terms, B, Condition, A));
        }

        /// <summary>
        /// Defines a bidirectional rule involving a function of two arguments that allows the order of the arguments to be swapped. Additional terms may
        /// be used when describing the function.
        /// </summary>
        public void DefineCommutativeProperty(int Terms, Expression Function)
        {
            int i = Terms;
            Term a = Term.Get(i);
            Term b = Term.Get(i + 1);
            this.DefineRule(Terms + 2, Function + a + b, Function + b + a);
        }

        /// <summary>
        /// Defines a rules that allows an expression of the form given by the template to be replaced with the form of the result using terms as
        /// placeholders for more specialized expressions.
        /// </summary>
        public void DefineRule(int Terms, Expression Template, Expression Result)
        {
            this.DefineRule(Terms, Template, Result, Bool.True);
        }

        /// <summary>
        /// Defines a rules that allows an expression of the form given by the template to be replaced with the form of the result when the condition 
        /// is true using terms as placeholders for more specialized expressions.
        /// </summary>
        public void DefineRule(int Terms, Expression Template, Expression Result, Expression Condition)
        {
            this.DefineRule(new SimpleRule(Terms, Template, Condition, Result));
        }

        private List<Rule> _Rules;
    }
}