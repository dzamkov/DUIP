using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type for unicode strings.
    /// </summary>
    public class StringType : Type
    {
        private StringType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly StringType Singleton = new StringType();

        public override bool Equal(object A, object B)
        {
            return A as string == B as string;
        }

        public override ISerialization<object> GetSerialization(Context Context)
        {
            return new StringSerialization();
        }

        public override UI.Block CreateBlock(object Instance, UI.Theme Theme)
        {
            return Theme.TextBlock(Instance as string);
        }
    }

    /// <summary>
    /// A serialization method for strings.
    /// </summary>
    public class StringSerialization : ISerialization<string>, ISerialization<object>
    {
        public void Serialize(string Object, OutStream Stream)
        {
            throw new NotImplementedException();
        }

        public string Deserialize(InStream Stream)
        {
            throw new NotImplementedException();
        }

        void ISerialization<object>.Serialize(object Object, OutStream Stream)
        {
            throw new NotImplementedException();
        }

        object ISerialization<object>.Deserialize(InStream Stream)
        {
            throw new NotImplementedException();
        }

        public Maybe<long> Size
        {
            get
            {
                return Maybe<long>.Nothing;
            }
        }
    }
}