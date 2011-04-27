using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.Lang.Parse
{
    /// <summary>
    /// A finite string of text.
    /// </summary>
    public abstract class Text
    {
        /// <summary>
        /// Gets the character at the given index in the text.
        /// </summary>
        public abstract char this[int Index] { get; }
        
        /// <summary>
        /// Gets the length, in characters, of the text.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets a string representation of this text.
        /// </summary>
        public abstract string String { get; }

        /// <summary>
        /// Gets a portion of this text.
        /// </summary>
        public virtual Text Sub(int Offset, int Length)
        {
            return new SubText(this, Offset, Length);
        }

        /// <summary>
        /// Gets the rightmost portion of this text beginning at the given offset.
        /// </summary>
        public Text Sub(int Offset)
        {
            return new SubText(this, Offset, this.Length - Offset);
        }

        public static implicit operator Text(string String)
        {
            return new StringText(String);
        }

        public static implicit operator string(Text Text)
        {
            return Text.String;
        }
    }

    /// <summary>
    /// A portion of some source text.
    /// </summary>
    public class SubText : Text
    {
        public SubText(Text Source, int Offset, int Length)
        {
            this._Source = Source;
            this._Offset = Offset;
            this._Length = Length;
        }

        /// <summary>
        /// Gets the source text.
        /// </summary>
        public Text Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the offset from the source text this text begins at.
        /// </summary>
        public int Offset
        {
            get
            {
                return this._Offset;
            }
        }

        public override string String
        {
            get
            {
                char[] cs = new char[this._Length];
                for (int t = 0; t < cs.Length; t++)
                {
                    cs[t] = this._Source[this._Offset + t];
                }
                return new string(cs);
            }
        }

        public override int Length
        {
            get
            {
                return this._Length;
            }
        }

        public override char this[int Index]
        {
            get
            {
                return this._Source[Index + this._Offset];
            }
        }

        public override Text Sub(int Offset, int Length)
        {
            return new SubText(this._Source, this._Offset + Offset, Length);    
        }

        private Text _Source;
        private int _Offset;
        private int _Length;
    }

    /// <summary>
    /// Text from a string (array of chars).
    /// </summary>
    public class StringText : Text
    {
        public StringText(string Source)
        {
            this._Source = Source;
        }

        /// <summary>
        /// Gets the source string for this text.
        /// </summary>
        public string Source
        {
            get
            {
                return this._Source;
            }
        }

        public override string String
        {
            get
            {
                return this._Source;
            }
        }

        public override int Length
        {
            get
            {
                return this._Source.Length;
            }
        }

        public override char this[int Index]
        {
            get
            {
                return this._Source[Index];
            }
        }

        private string _Source;
    }

    /// <summary>
    /// A method of parsing text into a logical object of the given type.
    /// </summary>
    public abstract class Parser<T>
    {
        /// <summary>
        /// Greedily parses the given text. Returns true, modifies Text to reflect the remaning the text and
        /// modifies Object to be the parsed object on success. Returns false and leaves Text unchanged if the text can not be parsed. 
        /// </summary>
        public abstract bool Accept(ref Text Text, ref T Object);

        /// <summary>
        /// Parses the given text, ignoring the result.
        /// </summary>
        public bool Accept(ref Text Text)
        {
            T dummy = default(T);
            return this.Accept(ref Text, ref dummy);
        }

        /// <summary>
        /// Creates a decorated form of this parser that only accepts object that can pass through the given filter.
        /// </summary>
        public Parser<T> Filter(Func<T, bool> IsAllowed)
        {
            return new FilterParser<T>(this, IsAllowed);
        }

        /// <summary>
        /// Creates a decorated form of this parser that maps (transforms) results.
        /// </summary>
        public Parser<F> Map<F>(Func<T, F> Map)
        {
            return new MapParser<T, F>(this, Map);
        }

        /// <summary>
        /// Creates a decorated form of this parser that accepts zero or more objects.
        /// </summary>
        public Parser<List<T>> Multi
        {
            get
            {
                return new MultiParser<T>(this);
            }
        }

        /// <summary>
        /// Creates a decorated form of this parser that accepts one or more objects.
        /// </summary>
        public Parser<List<T>> Many
        {
            get
            {
                return new MultiParser<T>(this).Filter(x => x.Count > 0);
            }
        }

        /// <summary>
        /// Creates a decorated form of this parser that accepts zero or one objects.
        /// </summary>
        public Parser<Maybe<T>> Possible
        {
            get
            {
                return new PossibleParser<T>(this);
            }
        }

        /// <summary>
        /// Creates a decorated form of this parser that accepts zero or more objects seperated by a delimiter.
        /// </summary>
        public Parser<List<T>> Delimit(Parser<Void> Delimiter)
        {
            return new DelimitedParser<T>(this, Delimiter);
        }

        /// <summary>
        /// Creates a parser which requires an object from this and another parser immediately following in order to
        /// accept text.
        /// </summary>
        public Parser<Tuple<T, TB>> Concat<TB>(Parser<TB> Other)
        {
            return new ConcatParser<T, TB>(this, Other);
        }

        /// <summary>
        /// Creates a parser which requires an object from this and the given text immediately following.
        /// </summary>
        public Parser<T> ConcatItem(Text Item)
        {
            return this.ConcatIgnore(Parser.Item(Item));
        }

        /// <summary>
        /// Creates a parser which requires an object from this and another parser immediately following in order to
        /// accept text. The result of the second parser is ignored.
        /// </summary>
        public Parser<T> ConcatIgnore<TB>(Parser<TB> Other)
        {
            return new ConcatParser<T, TB>(this, Other).Map(x => x.A);
        }

        public static Parser<T> operator +(Parser<T> Primary, Parser<T> Secondary)
        {
            return new UnionParser<T>(Primary, Secondary);
        }
    }

    /// <summary>
    /// Contains helper functions related to Parsers.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Gets a parser for whitespace.
        /// </summary>
        public static WhiteSpaceParser WhiteSpace
        {
            get
            {
                return WhiteSpaceParser.Singleton;
            }
        }

        /// <summary>
        /// Gets a parser for a expression.
        /// </summary>
        public static Parser<Expression> Expression
        {
            get
            {
                return DUIP.Lang.Parse.Expression.ExpressionParser;
            }
        }

        /// <summary>
        /// Gets a parser for a statement.
        /// </summary>
        public static Parser<Statement> Statement
        {
            get
            {
                return DUIP.Lang.Parse.Statement.StatementParser;
            }
        }

        /// <summary>
        /// Gets a parser for a word (valid variable or keyword).
        /// </summary>
        public static Parser<Text> Word
        {
            get
            {
                return WordParser.Singleton;
            }
        }

        /// <summary>
        /// Creates a parser that only accepts the given text.
        /// </summary>
        public static ItemParser Item(Text Target)
        {
            return new ItemParser(Target);
        }
    }

    /// <summary>
    /// Parses zero or more objects of with certain item parser.
    /// </summary>
    public class MultiParser<T> : Parser<List<T>>
    {
        public MultiParser(Parser<T> Item)
        {
            this._Item = Item;
        }

        /// <summary>
        /// The parser for used for an item in the list.
        /// </summary>
        public Parser<T> Item
        {
            get
            {
                return this._Item;
            }
        }

        public override bool Accept(ref Text Text, ref List<T> Object)
        {
            Object = new List<T>();
            T obj = default(T);
            while (this._Item.Accept(ref Text, ref obj))
            {
                Object.Add(obj);
            }
            return true;
        }

        private Parser<T> _Item;
    }

    /// <summary>
    /// Parses zero or one objects of with certain source parser.
    /// </summary>
    public class PossibleParser<T> : Parser<Maybe<T>>
    {
        public PossibleParser(Parser<T> Source)
        {
            this._Source = Source;
        }

        /// <summary>
        /// The source parser.
        /// </summary>
        public Parser<T> Source
        {
            get
            {
                return this._Source;
            }
        }

        public override bool Accept(ref Text Text, ref Maybe<T> Object)
        {
            T obj = default(T);
            if (this._Source.Accept(ref Text, ref obj))
            {
                Object = Maybe<T>.Just(obj);
            }
            else
            {
                Object = Maybe<T>.Nothing;
            }
            return true;
        }

        private Parser<T> _Source;
    }

    /// <summary>
    /// Filters results from a source parser.
    /// </summary>
    public class FilterParser<T> : Parser<T>
    {
        public FilterParser(Parser<T> Source, Func<T, bool> IsAllowed)
        {
            this._Source = Source;
            this._IsAllowed = IsAllowed;
        }

        /// <summary>
        /// Gets the source (decorated) parser.
        /// </summary>
        public Parser<T> Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the function used to determine wether an object is allowed through the filter.
        /// </summary>
        public Func<T, bool> IsAllowed
        {
            get
            {
                return this._IsAllowed;
            }
        }

        public override bool Accept(ref Text Text, ref T Object)
        {
            Text o = Text;
            if (this._Source.Accept(ref Text, ref Object))
            {
                if (this._IsAllowed(Object))
                {
                    return true;
                }
            }
            Text = o;
            return false;
        }

        private Parser<T> _Source;
        private Func<T, bool> _IsAllowed;
    }

    /// <summary>
    /// Maps (transforms) results from a source parser.
    /// </summary>
    public class MapParser<T, F> : Parser<F>
    {
        public MapParser(Parser<T> Source, Func<T, F> Map)
        {
            this._Source = Source;
            this._Map = Map;
        }

        /// <summary>
        /// Gets the source (decorated) parser.
        /// </summary>
        public Parser<T> Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the mapping (transformation) function used.
        /// </summary>
        public Func<T, F> Map
        {
            get
            {
                return this._Map;
            }
        }

        public override bool Accept(ref Text Text, ref F Object)
        {
            T obj = default(T);
            if (this._Source.Accept(ref Text, ref obj))
            {
                Object = this._Map(obj);
                return true;
            }
            else
            {
                return false;
            }
        }

        private Parser<T> _Source;
        private Func<T, F> _Map;
    }

    /// <summary>
    /// A parser that uses two parser for parsing text.
    /// </summary>
    public class UnionParser<T> : Parser<T>
    {
        public UnionParser(Parser<T> Primary, Parser<T> Secondary)
        {
            this._Primary = Primary;
            this._Secondary = Secondary;
        }

        /// <summary>
        /// Gets the primary parser. This parser is tried first and its
        /// results will be given if it succeeds.
        /// </summary>
        public Parser<T> Primary
        {
            get
            {
                return this._Primary;
            }
        }

        /// <summary>
        /// Gets the secondary parser. This parser is tried after the primary parser
        /// and is only used if the primary parser fails.
        /// </summary>
        public Parser<T> Secondary
        {
            get
            {
                return this._Secondary;
            }
        }

        public override bool Accept(ref Text Text, ref T Object)
        {
            if (this._Primary.Accept(ref Text, ref Object))
            {
                return true;
            }
            if (this._Secondary.Accept(ref Text, ref Object))
            {
                return true;
            }
            return false;
        }

        private Parser<T> _Primary;
        private Parser<T> _Secondary;
    }

    /// <summary>
    /// A heterogeneous structure of two unnamed items.
    /// </summary>
    public struct Tuple<TA, TB>
    {
        public Tuple(TA A, TB B)
        {
            this.A = A;
            this.B = B;
        }

        public TA A;
        public TB B;
    }

    /// <summary>
    /// A parser that accepts two parsed objects in series.
    /// </summary>
    public class ConcatParser<TA, TB> : Parser<Tuple<TA, TB>>
    {
        public ConcatParser(Parser<TA> A, Parser<TB> B)
        {
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// The parser used for the first part of this parser.
        /// </summary>
        public Parser<TA> A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// The parser used for the second part of this parser. Will be called only be called
        /// if a match for A is found.
        /// </summary>
        public Parser<TB> B
        {
            get
            {
                return this._B;
            }
        }

        public override bool Accept(ref Text Text, ref Tuple<TA, TB> Object)
        {
            Text o = Text;
            TA obja = default(TA);
            TB objb = default(TB);
            if (this._A.Accept(ref Text, ref obja))
            {
                if (this._B.Accept(ref Text, ref objb))
                {
                    Object = new Tuple<TA, TB>(obja, objb);
                    return true;
                }
            }
            Text = o;
            return false;
        }

        private Parser<TA> _A;
        private Parser<TB> _B;
    }

    /// <summary>
    /// A parser that only accepts a given text.
    /// </summary>
    public class ItemParser : Parser<Void>
    {
        public ItemParser(Text Target)
        {
            this._Target = Target;
        }

        /// <summary>
        /// Gets the text this parser will accept.
        /// </summary>
        public Text Target
        {
            get
            {
                return this._Target;
            }
        }

        public override bool Accept(ref Text Text, ref Void Object)
        {
            int len = this._Target.Length;
            if (Text.Length < len)
            {
                return false;
            }
            for (int t = 0; t < len; t++)
            {
                if (Text[t] != this._Target[t])
                {
                    return false;
                }
            }
            Text = Text.Sub(len);
            return true;
        }

        private Text _Target;
    }

    /// <summary>
    /// Parses zero or more objects seperated by a delimiter.
    /// </summary>
    public class DelimitedParser<T> : Parser<List<T>>
    {
        public DelimitedParser(Parser<T> Item, Parser<Void> Delimiter)
        {
            this._Item = Item;
            this._Delimiter = Delimiter;
        }

        /// <summary>
        /// The parser for an item.
        /// </summary>
        public Parser<T> Item
        {
            get
            {
                return this._Item;
            }
        }

        /// <summary>
        /// The parser for the delimiter.
        /// </summary>
        public Parser<Void> Delimiter
        {
            get
            {
                return this._Delimiter;
            }
        }

        public override bool Accept(ref Text Text, ref List<T> Object)
        {
            Object = new List<T>();
            T nobj = default(T);
            if (this._Item.Accept(ref Text, ref nobj))
            {
                Object.Add(nobj);

                while (true)
                {
                    Text o = Text;
                    if (this._Delimiter.Accept(ref o))
                    {
                        if (this._Item.Accept(ref o, ref nobj))
                        {
                            Text = o;
                            Object.Add(nobj);
                            continue;
                        }
                    }
                    break;
                }
            }
            return true;
        }

        private Parser<T> _Item;
        private Parser<Void> _Delimiter;
    }
}