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
    }
}