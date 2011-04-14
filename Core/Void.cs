using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A dataless object with only one value.
    /// </summary>
    public struct Void
    {
        /// <summary>
        /// Gets the only value of this object.
        /// </summary>
        public static Void Value = new Void();
    }

    /// <summary>
    /// A type with only one instance that can be stored without any data.
    /// </summary>
    public class VoidType : Type<Void>
    {
        private VoidType()
        {

        }

        /// <summary>
        /// Gets the only instance of this class.
        /// </summary>
        public static readonly VoidType Singleton = new VoidType();

        public override void Serialize(Context Context, Void Instance, OutStream Stream)
        {

        }

        public override Query<Void> Deserialize(Context Context, InStream Stream)
        {
            return DUIP.Void.Value;
        }

        protected override void SerializeType(Context Context, OutStream Stream)
        {
            Stream.Write((byte)TypeMode.Void);
        }

        public override string Name
        {
            get
            {
                return "void";
            }
        }

        public override Maybe<Void> Parse(string Value)
        {
            return DUIP.Void.Value;
        }
    }
}