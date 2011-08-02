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

        /// <summary>
        /// Gets a task that tries evaluating the given expression to get a literal of the given type. If this is not possible, null is returned. It is assumed
        /// that the expression is of the correct type to have a literal of the given type.
        /// </summary>
        public Task<Maybe<T>> Evaluate<T>(Expression Expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a task tries to convert the value given by an expression to an instance of the given pattern. Since there may be multiple solution to this problem,
        /// it is possible to continue searching for instances after the first is found.
        /// </summary>
        public Task<Maybe<Form>> Transform(Expression Expression, Pattern Pattern)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A result from an execution of a Transform function.
        /// </summary>
        public struct Form
        {
            /// <summary>
            /// The values for the terms in the instance of the pattern.
            /// </summary>
            public Expression[] Instance;

            /// <summary>
            /// The task to continue searching for forms. The form returned by this task will be distinct from this form, or any previous forms returned.
            /// </summary>
            public Task<Maybe<Form>> Continue;
        }

        private List<Rule> _Rules;
    }
}