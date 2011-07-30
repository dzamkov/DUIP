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

        public override Expression Type
        {
            get
            {
                return this._Type;
            }
        }

        /// <summary>
        /// Sets the type of this symbol. This may only be called once, and can only be called if a type was not included
        /// in the constructor for the symbol.
        /// </summary>
        public void SetType(Expression Type)
        {
            this._Type = Type;
        }

        private Expression _Type;
    }

}