using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A figure that takes the appearance of a source figure, but offers some hint on how to render
    /// or manage it. Hints may be ignored if they are not understood.
    /// </summary>
    public abstract class HintFigure : Figure
    {
        public HintFigure(Figure Source)
        {
            this.Source = Source;
        }

        /// <summary>
        /// The source figure the hint is applied to.
        /// </summary>
        public readonly Figure Source;
    }

    /// <summary>
    /// A hint figure that gives an estimate of the number of times the figure will be rendered. This is useful
    /// in determining wether to cache the rendering operations needed to render a figure.
    /// </summary>
    public sealed class PersistenceHintFigure : HintFigure
    {
        public PersistenceHintFigure(int NumberRenders, Figure Source)
            : base(Source)
        {
            this.NumberRenders = NumberRenders;
        }

        /// <summary>
        /// An estimate of the number of times this figure will be rendered before being discarded.
        /// </summary>
        public readonly int NumberRenders;
    }

    /// <summary>
    /// A hint figure that indicates that the information needed to render the source figure should be cached between program runs. The
    /// figure is identified with a name and is expected to be the same every time this hint is used with that name.
    /// </summary>
    public sealed class CacheHintFigure : HintFigure
    {
        public CacheHintFigure(string Name, Figure Source)
            : base(Source)
        {
            this.Name = Name;
        }

        /// <summary>
        /// The name used to identify the source figure.
        /// </summary>
        public readonly string Name;
    }
}