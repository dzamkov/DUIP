using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Lang.Parse
{
    /// <summary>
    /// A reference to a constant value.
    /// </summary>
    public enum Constant
    {
        True,
        False,
        Not,
        And,
        Or,
        Xor,

        Plus,
        Minus,
        Times,
        Divide,


    }

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
        /// Gets the item delimiter for collections such as tuples, lists, sets and maps.
        /// </summary>
        public static Parser<Void> CollectionDelimiter
        {
            get
            {
                return _CollectionDelimiter ?? (_CollectionDelimiter =
                        Parser.WhiteSpace.Possible
                        .ConcatItem(",")
                        .ConcatIgnore(Parser.WhiteSpace.Possible)
                        .Map<Void>(x => Void.Value)
                    );
            }
        }
        private static Parser<Void> _CollectionDelimiter;

        /// <summary>
        /// A parser for a collection of items.
        /// </summary>
        public static Parser<List<T>> CollectionParser<T>(Text Start, Text End, Parser<T> Item)
        {
            return Parser.Item(Start)
            .ConcatIgnore(Parser.WhiteSpace.Possible)
            .Concat(Item.Delimit(CollectionDelimiter, true))
            .ConcatIgnore(Parser.WhiteSpace.Possible)
            .ConcatItem(End).Map(x => x.B);
        }

        /// <summary>
        /// Given a list of expression (given by the user) creates an appropriate tuple collection to contain them.
        /// </summary>
        public static Expression MakeTuple(List<Expression> Items, bool Type)
        {
            if (Items.Count == 1)
            {
                return Items[0];
            }
            else
            {
                if (Type)
                {
                    return new TupleTypeExpression()
                    {
                        Parts = Items
                    };
                }
                else
                {
                    return new TupleExpression()
                    {
                        Parts = Items
                    };
                }
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

                        // Tuple or Expression in parentheses
                        CollectionParser("(", ")", ExpressionParser)
                        .Map<Expression>(x => MakeTuple(x, false)) +

                        // Tuple type
                        CollectionParser("<", ">", ExpressionParser)
                        .Map<Expression>(x => MakeTuple(x, true)) +

                        // List
                        CollectionParser("[", "]", ExpressionParser)
                        .Map<Expression>(x => new ListExpression() { Items = x }) +

                        // Set
                        CollectionParser("{", "}", ExpressionParser)
                        .Map<Expression>(x => new SetExpression() { Items = x }) +

                        // Map
                        CollectionParser("{", "}", ExpressionParser
                            .ConcatIgnore(Parser.WhiteSpace)
                            .ConcatItem(":")
                            .ConcatIgnore(Parser.WhiteSpace)
                            .Concat(ExpressionParser))
                        .Map<Expression>(x => new MapExpression()
                        {
                            Items = new List<KeyValuePair<Expression, Expression>>(
                                from p in x
                                select new KeyValuePair<Expression, Expression>(p.A, p.B)
                            )
                        })
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
                    Text o = Text;
                    Parser.WhiteSpace.Accept(ref o);

                    // Accessor
                    if (Parser.WhiteSpace.ConcatItem(".").Accept(ref o))
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
                    if (Parser.Item("?").Accept(ref o))
                    {
                        Text = o;
                        Object = new BindExpression()
                        {
                            Source = Object
                        };
                        continue;
                    }

                    // Function call 
                    List<Expression> pars = null;
                    if (Expression.CollectionParser("(", ")", Expression.ExpressionParser).Accept(ref o, ref pars))
                    {
                        Text = o;
                        Object = new FunctionCallExpression()
                        {
                            Function = Object,
                            Argument = Expression.MakeTuple(pars, false)
                        };
                        continue;
                    }

                    // Function call with tuple type
                    if (Expression.CollectionParser("<", ">", Expression.ExpressionParser).Accept(ref o, ref pars))
                    {
                        Text = o;
                        Object = new FunctionCallExpression()
                        {
                            Function = Object,
                            Argument = Expression.MakeTuple(pars, true)
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
    /// An expression that evaluates to a constant value.
    /// </summary>
    public class ConstantExpression : Expression
    {
        /// <summary>
        /// The type of this constant.
        /// </summary>
        public Constant Constant;
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
    /// An expression that builds a tuple type using a seperate expression for each part.
    /// </summary>
    public class TupleTypeExpression : Expression
    {
        /// <summary>
        /// The expressions for the parts of the tuple type.
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