using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A reference to a value that is not immediately defined. Symbol that have equivalent references
    /// are considered to have an equivalent value.
    /// </summary>
    public sealed class Symbol : Expression
    {
        public Symbol()
        {

        }

        public Symbol(Expression Type)
        {
            this._Type = Type;
        }

        public Symbol(string Name)
        {
            this._Name = Name;
        }

        public Symbol(Expression Type, string Name)
        {
            this._Type = Type;
            this._Name = Name;
        }

        /// <summary>
        /// Gets the prefered name for this symbol. If this is null, the symbol does not have a name.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public override Expression Type
        {
            get
            {
                return this._Type;
            }
        }

        public override Expression Fill(Expression[] Terms)
        {
            return this;
        }

        public override string ToString()
        {
            return this._Name ?? this.GetHashCode().ToString();
        }

        /// <summary>
        /// Sets the type of this symbol. This may only be called once, and can only be called if a type was not included
        /// in the constructor for the symbol.
        /// </summary>
        public void SetType(Expression Type)
        {
            this._Type = Type;
        }

        private string _Name;
        private Expression _Type;
    }

}