using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
{
    /// <summary>
    /// A (possibly mutable) description of a renderable image on an infinitely large plane.
    /// </summary>
    public abstract class Figure
    {
        /// <summary>
        /// Renders the figure using the given context.
        /// </summary>
        public abstract void Render(RenderContext Context);

        /// <summary>
        /// Gets a rectangle such that all points outside the rectangle are completely transparent (have an
        /// alpha value of 0).
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Unbound;
            }
        }

        /// <summary>
        /// Translates this figure by the given offset.
        /// </summary>
        public virtual Figure Translate(Point Offset)
        {
            return new TranslatedFigure(this, Offset);
        }
    }

    /// <summary>
    /// A translated form of another figure.
    /// </summary>
    public class TranslatedFigure : Figure
    {
        public TranslatedFigure(Figure Source, Point Translation)
        {
            this._Source = Source;
            this._Translation = Translation;
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
        public Point Translation
        {
            get
            {
                return this._Translation;
            }
        }

        public override void Render(RenderContext Context)
        {
            using (Context.Translate(this._Translation))
            {
                this._Source.Render(Context);
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                return this._Source.Bounds.Translate(this._Translation);
            }
        }

        public override Figure Translate(Point Offset)
        {
            return new TranslatedFigure(this._Source, this._Translation + Offset);
        }

        private Figure _Source;
        private Point _Translation;
    }

    /// <summary>
    /// A figure made from the combination of multiple component figures.
    /// </summary>
    public class GroupFigure : Figure
    {
        public GroupFigure(IEnumerable<Figure> Components)
        {
            this._Components = Components;
        }

        /// <summary>
        /// Gets the components of this figure.
        /// </summary>
        public IEnumerable<Figure> Components
        {
            get
            {
                return this._Components;
            }
        }

        public override void Render(RenderContext Context)
        {
            foreach (Figure fig in this._Components)
            {
                fig.Render(Context);
            }
        }

        private IEnumerable<Figure> _Components;
    }
}