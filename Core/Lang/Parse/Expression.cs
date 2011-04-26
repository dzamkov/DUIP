using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Lang.Parse
{
    /// <summary>
    /// A parsed expression that retains variable names. This provides an intermediate form of an expression between
    /// human readable text and executable and evaluatable expressions. Note that unlike evaluatable expressions, these
    /// expression may have side effects to the local block.
    /// </summary>
    public class Expression
    {
        /// <summary>
        /// Gets a parser for an expression.
        /// </summary>
        public static Parser<Expression> ExpressionParser
        {
            get
            {
                return TightExpressionParser;
            }
        }

        /// <summary>
        /// Gets a parser for an atomic expression (a tight expression that does not contain any accessors, calls or post-modifiers on its top level).
        /// </summary>
        public static Parser<Expression> AtomicExpressionParser
        {
            get
            {
                return _AtomicExpressionParser ?? (_AtomicExpressionParser =
                        // Variable
                        Parser.Word.
                        Map<Expression>(x => new VariableExpression()
                        {
                            Name = x
                        }) +

                        // Expression in parentheses
                        Parser.
                        Item("(").
                        ConcatIgnore(Parser.WhiteSpace).
                        Concat(ExpressionParser).
                        ConcatIgnore(Parser.WhiteSpace).
                        ConcatItem(")").
                        Map<Expression>(x => x.B)
                    );
            }
        }
        private static Parser<Expression> _AtomicExpressionParser;

        /// <summary>
        /// Gets a parser for a tight expression (an expression that can not be broken into sections by an operator).
        /// </summary>
        public static Parser<Expression> TightExpressionParser
        {
            get
            {
                return DUIP.Lang.Parse.TightExpressionParser.Singleton;
            }
        }
    }

    /// <summary>
    /// A parser for a tight expression.
    /// </summary>
    public class TightExpressionParser : Parser<Expression>
    {
        private TightExpressionParser()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly TightExpressionParser Singleton = new TightExpressionParser();

        public override bool Accept(ref Text Text, ref Expression Object)
        {
            // Start with an atom
            if (Expression.AtomicExpressionParser.Accept(ref Text, ref Object))
            {
                // Look for accessors, calls or post-modifiers
                while (true)
                {
                    // Accessor
                    Text o = Text;
                    if (Parser.Item(".").Accept(ref o))
                    {
                        Text name = null;
                        if (Parser.Word.Accept(ref o, ref name))
                        {
                            Text = o;
                            Object = new AccessorExpression()
                            {
                                Object = Object,
                                Property = name
                            };
                            continue;
                        }
                    }

                    // Bind modifier
                    if (Parser.Item("?").Accept(ref Text))
                    {
                        Object = new BindExpression()
                        {
                            Source = Object
                        };
                        continue;
                    }

                    break;
                }
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// A reference to a variable.
    /// </summary>
    public class VariableExpression : Expression
    {
        /// <summary>
        /// The name of the variable referenced.
        /// </summary>
        public Text Name;
    }

    /// <summary>
    /// An expression representing a function call.
    /// </summary>
    public class FunctionCallExpression : Expression
    {
        /// <summary>
        /// The function that is called.
        /// </summary>
        public Expression Function;

        /// <summary>
        /// The argument given to the function for the call.
        /// </summary>
        public Expression Argument;
    }

    /// <summary>
    /// An expression representing an invocation of a statement or group of statements. Expressions
    /// of this type are written in an imperative style.
    /// </summary>
    public class BlockExpression : Expression
    {
        /// <summary>
        /// Contains the monadic properties of the block, or null if the block is not monadic.
        /// </summary>
        public Expression Monad;

        /// <summary>
        /// The statement to be invoked.
        /// </summary>
        public Statement Statement;
    }

    /// <summary>
    /// An expression that gets a property from the value of another expression.
    /// </summary>
    public class AccessorExpression : Expression
    {
        /// <summary>
        /// The object being "accessed".
        /// </summary>
        public Expression Object;

        /// <summary>
        /// The name of the property being "accessed".
        /// </summary>
        public Text Property;
    }

    /// <summary>
    /// An expression that builds a tuple using a seperate expression for each part.
    /// </summary>
    public class TupleExpression : Expression
    {
        /// <summary>
        /// The expressions for the parts of the tuple.
        /// </summary>
        public List<Expression> Parts;
    }

    /// <summary>
    /// An expression that builds a list (a list literal).
    /// </summary>
    public class ListExpression : Expression
    {
        /// <summary>
        /// The ordered items in the list.
        /// </summary>
        public List<Expression> Items;
    }

    /// <summary>
    /// An expression that builds a set (a set literal).
    /// </summary>
    public class SetExpression : Expression
    {
        /// <summary>
        /// The items in the set. Duplicate items will only be included once.
        /// </summary>
        public List<Expression> Items;
    }

    /// <summary>
    /// An expression that builds a map (a map literal).
    /// </summary>
    public class MapExpression : Expression
    {
        /// <summary>
        /// The items in the map. The last value will be used for the case of duplicate keys.
        /// </summary>
        public List<KeyValuePair<Expression, Expression>> Items;
    }

    /// <summary>
    /// An expression representing a bind in a monadic block.
    /// </summary>
    public class BindExpression : Expression
    {
        /// <summary>
        /// The monadic expression to bind the value of.
        /// </summary>
        public Expression Source;
    }

    /// <summary>
    /// An expression, that when evaluated, assigns a variable in the local scope to a value and
    /// returns the value.
    /// </summary>
    public class AssignExpression : Expression
    {
        /// <summary>
        /// The name of the variable to be assigned.
        /// </summary>
        public Text Variable;
        
        /// <summary>
        /// The value the variable is assigned to.
        /// </summary>
        public Expression Value;
    }
}