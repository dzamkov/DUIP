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
        /// Creates a translated form of a figure.
        /// </summary>
        public static TranslatedFigure Translate(Figure Figure, Point Offset)
        {
            if (Figure == null)
                return null;
            return new TranslatedFigure(Offset, Figure);
        }

        /// <summary>
        /// Creates a scaled form of a figure.
        /// </summary>
        public static ScaledFigure Scale(Figure Figure, double Factor)
        {
            if (Figure == null)
                return null;
            return new ScaledFigure(Factor, Figure);
        }

        /// <summary>
        /// Creates a rotated form of a figure.
        /// </summary>
        public static RotatedFigure Rotate(Figure Figure, double Angle)
        {
            if (Figure == null)
                return null;
            return new RotatedFigure(Angle, Figure);
        }

        /// <summary>
        /// Creates a modulated form of a figure.
        /// </summary>
        public static ModulatedFigure Modulate(Figure Figure, Color Modulation)
        {
            if (Figure == null)
                return null;
            return new ModulatedFigure(Modulation, Figure);
        }

        /// <summary>
        /// Creates a projected form of a figure.
        /// </summary>
        public static ProjectedFigure Project(Figure Figure, View Projection)
        {
            if (Figure == null)
                return null;
            return new ProjectedFigure(Projection, Figure);
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
            this.Source = Source;
            this.Offset = Offset;
        }

        /// <summary>
        /// The source figure that is translated.
        /// </summary>
        public readonly Figure Source;

        /// <summary>
        /// The translation applied to the source figure.
        /// </summary>
        public readonly Point Offset;
    }

    /// <summary>
    /// A (uniformly) scaled form of a figure.
    /// </summary>
    public sealed class ScaledFigure : Figure
    {
        public ScaledFigure(double Factor, Figure Source)
        {
            this.Factor = Factor;
            this.Source = Source;
        }

        /// <summary>
        /// The source figure that is scaled.
        /// </summary>
        public readonly Figure Source;

        /// <summary>
        /// The amount the source figure is scaled by.
        /// </summary>
        public readonly double Factor;
    }

    /// <summary>
    /// A rotated form of a figure.
    /// </summary>
    public sealed class RotatedFigure : Figure
    {
        public RotatedFigure(double Angle, Figure Source)
        {
            this.Angle = Angle;
            this.Source = Source;
        }

        /// <summary>
        /// The source figure that is rotated.
        /// </summary>
        public readonly Figure Source;

        /// <summary>
        /// The angle, in radians, that the source figure is rotated by (going counter-clockwise).
        /// </summary>
        public readonly double Angle;
    }

    /// <summary>
    /// A transformed form of a figure using a projection defined by a view.
    /// </summary>
    public sealed class ProjectedFigure : Figure
    {
        public ProjectedFigure(View Projection, Figure Source)
        {
            this.Source = Source;
            this.Projection = Projection;
        }

        /// <summary>
        /// The source figure that is projected.
        /// </summary>
        public readonly Figure Source;

        /// <summary>
        /// The projection applied to the source figure. The final position of any feature on the source
        /// figure is given by projecting (from view space to world space) the initial position using this view.
        /// </summary>
        public readonly View Projection;
    }

    /// <summary>
    /// A figure where all colors are multiplied by a certain modulation color.
    /// </summary>
    public sealed class ModulatedFigure : Figure
    {
        public ModulatedFigure(Color Modulation, Figure Source)
        {
            this.Modulation = Modulation;
            this.Source = Source;
        }

        /// <summary>
        /// The source figure that is modulated.
        /// </summary>
        public readonly Figure Source;

        /// <summary>
        /// The color that defines the modulation to apply.
        /// </summary>
        public readonly Color Modulation;
    }

    /// <summary>
    /// A figure created by superimposing one figure on top of another. The visibility of the lower figure
    /// is determined by the transparency of the upper figure.
    /// </summary>
    public sealed class SuperimposedFigure : Figure
    {
        public SuperimposedFigure(Figure Under, Figure Over)
        {
            this.Under = Under;
            this.Over = Over;
        }

        /// <summary>
        /// The lower figure.
        /// </summary>
        public readonly Figure Under;

        /// <summary>
        /// The higher figure.
        /// </summary>
        public readonly Figure Over;
    }

    /// <summary>
    /// A figure created by combining multiple component figures where the order of rendering does not matter.
    /// </summary>
    public sealed class CompoundFigure : Figure
    {
        public CompoundFigure(IEnumerable<Figure> Components)
        {
            this.Components = Components;
        }

        /// <summary>
        /// The components of this compound figure.
        /// </summary>
        public readonly IEnumerable<Figure> Components;
    }
}