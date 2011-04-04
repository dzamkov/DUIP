using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP
{
    /// <summary>
    /// Immutable, serializable data that can be stored across a network.
    /// </summary>
    public abstract class Content
    {
        /// <summary>
        /// Tries interpreting this content with a more specific type. Returns null if not possible.
        /// </summary>
        public abstract Query<T> As<T>()
            where T : Content;
    }

    /// <summary>
    /// Content known to be interpretable as a more specific type.
    /// </summary>
    public struct Content<T>
        where T : Content
    {
        public Content(Content Raw)
        {
            this.Raw = Raw;
        }

        /// <summary>
        /// Gets the content in typed form.
        /// </summary>
        public Query<T> Cast
        {
            get
            {
                return this.Raw.As<T>();
            }
        }

        public static implicit operator T(Content<T> Content)
        {
            return Content.Cast.Execute();
        }

        public static implicit operator Content(Content<T> Content)
        {
            return Content.Raw;
        }

        /// <summary>
        /// The raw untyped representation of this content.
        /// </summary>
        public Content Raw;
    }

    /// <summary>
    /// Represents a range of contents called instances. Each type can serialize and deserialize instances to and
    /// from byte streams.
    /// </summary>
    public abstract class Type : Content
    {
        /// <summary>
        /// Deserializes content of this type from the given byte stream. Returns null it is not possible to construct a valid
        /// instance using the given data.
        /// </summary>
        public abstract Content Deserialize(InByteStream Stream);

        /// <summary>
        /// Serializes a known instance of this type to a stream.
        /// </summary>
        public abstract void Serialize(Content Instance, OutByteStream Stream);

        /// <summary>
        /// Gets if the specified content is an instance of this type.
        /// </summary>
        public abstract bool IsInstance(Content Content);
    }

    /// <summary>
    /// A reference to a content datum on the current network.
    /// </summary>
    public abstract class Reference : Content
    {
        /// <summary>
        /// Gets the referenced datum.
        /// </summary>
        public abstract Query<ContentDatum> Datum { get; }
    }
}