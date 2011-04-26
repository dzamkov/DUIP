using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Lang.Parse
{
    /// <summary>
    /// A parsed statement that is part of a block.
    /// </summary>
    public class Statement
    {
        /// <summary>
        /// Gets a parser for a statement.
        /// </summary>
        public static Parser<Statement> StatementParser
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// A statement representing the return of a value from a block.
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>
        /// An expression that evaluates to the value returned by the statement.
        /// </summary>
        public Expression Value;
    }

    /// <summary>
    /// A statement that forces the evaluation of an expression that likely has side effects.
    /// </summary>
    public class EvaluateStatement : Statement
    {
        /// <summary>
        /// The expression to evaluate.
        /// </summary>
        public Expression Expression;
    }

    /// <summary>
    /// A statement that defines or redefines a variable.
    /// </summary>
    public class DefinitionStatement : Statement
    {
        /// <summary>
        /// An expression that evaluates to the type of the variable.
        /// </summary>
        public Expression Type;

        /// <summary>
        /// The name of the variable that is assigned.
        /// </summary>
        public Text Name;

        /// <summary>
        /// The value the variable is assigned to.
        /// </summary>
        public Expression Value;
    }

    /// <summary>
    /// A statement that acts as a permutation of simpler statements. Variables defined within a block
    /// may not be used outside the block.
    /// </summary>
    public class BlockStatement : Statement
    {
        /// <summary>
        /// The statements that make up this block.
        /// </summary>
        public List<Statement> Statements;
    }
}