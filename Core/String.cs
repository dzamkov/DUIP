using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// A type for unicode strings.
    /// </summary>
    [Kind(2)]
    public sealed class StringType : Type, ISerialization<string>, ISerialization<object>
    {
        private StringType()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static readonly StringType Instance = new StringType();

        public override bool Equal(object A, object B)
        {
            return A as string == B as string;
        }

        public override ISerialization<object> Serialization
        {
            get 
            { 
                return this; 
            }
        }

        public void Write(ref string Object, OutStream Stream)
        {
            throw new NotImplementedException();
        }

        public new string Read(InStream Stream)
        {
            throw new NotImplementedException();
        }

        void ISerialization<object>.Write(ref object Object, OutStream Stream)
        {
            throw new NotImplementedException();
        }

        object ISerialization<object>.Read(InStream Stream)
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

        public override UI.Block CreateBlock(object Instance, UI.Theme Theme)
        {
            return Theme.TextBlock(Instance as string);
        }
    }
}