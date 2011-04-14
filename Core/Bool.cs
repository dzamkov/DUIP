using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type whose two instances are true and false.
    /// </summary>
    public class BoolType : Type<bool>
    {
        private BoolType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly BoolType Singleton = new BoolType();

        public override void Serialize(Context Context, bool Instance, OutStream Stream)
        {
            Stream.WriteBool(Instance);
        }

        public override Query<bool> Deserialize(Context Context, InStream Stream)
        {
            return Stream.ReadBool();
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Bool);
        }

        public override string Name
        {
            get
            {
                return "bool";
            }
        }

        public override Maybe<bool> Parse(string Value)
        {
            string l = Value.Trim().ToLower();
            if (l == "t" || l == "true")
            {
                return true;
            }
            if (l == "f" || l == "false")
            {
                return false;
            }
            return Maybe<bool>.Nothing;
        }
    }
}