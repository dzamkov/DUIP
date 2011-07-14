using System;
using System.Collections.Generic;
using System.Drawing;

namespace DUIP.UI
{
    /// <summary>
    /// A static description of an image on an infinitely large and precise plane.
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

        public static SuperimposedFigure operator +(Figure Under, Figure Over)
        {
            return new SuperimposedFigure(Under, Over);
        }

        public static CombinedFigure operator ^(Figure A, Figure B)
        {
            return new CombinedFigure(A, B);
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
    /// A figure created by superimposing one figure on top of another. The visibility of the lower figure
    /// is determined by the transparency of the upper figure.
    /// </summary>
    public sealed class SuperimposedFigure
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
    /// A figure created by combining two disjoint figures where every visible part in one figure
    /// is completely transparent in the other. This is similar to a superimposed figure where
    /// the order of the component figures does not matter. If the component figures are somehow not disjoint,
    /// the appearance of resulting figure is undefined.
    /// </summary>
    public sealed class CombinedFigure : Figure
    {
        public CombinedFigure(Figure A, Figure B)
        {
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Gets the first component of the combined figure.
        /// </summary>
        public Figure A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets the second component of the combined figure.
        /// </summary>
        public Figure B
        {
            get
            {
                return this._B;
            }
        }

        private Figure _A;
        private Figure _B;
    }
}