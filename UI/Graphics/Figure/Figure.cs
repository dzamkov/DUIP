using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A static description of an image on an infinitely large and precise plane. A null figure indicates
    /// a completely transparent image.
    /// </summary>
    public abstract class Figure
    {
        /// <summary>
        /// Creates a translated form of this figure.
        /// </summary>
        public TranslatedFigure Translate(Point Offset)
        {
            return new TranslatedFigure(Offset, this);
        }

        /// <summary>
        /// Creates a projected form of this figure.
        /// </summary>
        public ProjectedFigure Project(View Projection)
        {
            return new ProjectedFigure(Projection, this);
        }

        public static Figure operator +(Figure Under, Figure Over)
        {
            if (Under == null)
                return Over;
            if (Over == null)
                return Under;
            return new SuperimposedFigure(Under, Over);
        }

        public static Figure operator ^(Figure A, Figure B)
        {
            if (A == null)
                return B;
            if (B == null)
                return A;
            return new CompoundFigure(new Figure[] { A, B });
        }
    }

    /// <summary>
    /// A translated form of a figure.
    /// </summary>
    public sealed class TranslatedFigure : Figure
    {
        public TranslatedFigure(Point Offset, Figure Source)
        {
            this._Source = Source;
            this._Offset = Offset;
        }

        /// <summary>
        /// Gets the source figure that is translated.
        /// </summary>
        public Figure Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the translation applied to the source figure.
        /// </summary>
        public Point Offset
        {
            get
            {
                return this._Offset;
            }
        }

        private Figure _Source;
        private Point _Offset;
    }

    /// <summary>
    /// A transformed form of a figure using a projection defined by a view.
    /// </summary>
    public sealed class ProjectedFigure : Figure
    {
        public ProjectedFigure(View Projection, Figure Source)
        {
            this._Source = Source;
            this._Projection = Projection;
        }

        /// <summary>
        /// Gets the source figure that is projected.
        /// </summary>
        public Figure Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the projection applied to the source figure. The final position of any feature on the source
        /// figure is given by projecting (from view space to world space) the initial position using this view.
        /// </summary>
        public View Projection
        {
            get
            {
                return this._Projection;
            }
        }

        private Figure _Source;
        private View _Projection;
    }

    /// <summary>
    /// A figure created by superimposing one figure on top of another. The visibility of the lower figure
    /// is determined by the transparency of the upper figure.
    /// </summary>
    public sealed class SuperimposedFigure : Figure
    {
        public SuperimposedFigure(Figure Under, Figure Over)
        {
            this._Under = Under;
            this._Over = Over;
        }

        /// <summary>
        /// Gets the lower figure.
        /// </summary>
        public Figure Under
        {
            get
            {
                return this._Under;
            }
        }

        /// <summary>
        /// Gets the higher figure.
        /// </summary>
        public Figure Over
        {
            get
            {
                return this._Over;
            }
        }

        private Figure _Under;
        private Figure _Over;
    }

    /// <summary>
    /// A figure created by combining multiple component figures where the order of rendering does not matter.
    /// </summary>
    public sealed class CompoundFigure : Figure
    {
        public CompoundFigure(IEnumerable<Figure> Components)
        {
            this._Components = Components;
        }

        /// <summary>
        /// Gets the components of this compound figure.
        /// </summary>
        public IEnumerable<Figure> Components
        {
            get
            {
                return this._Components;
            }
        }

        private IEnumerable<Figure> _Components;
    }
}