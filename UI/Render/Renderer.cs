using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI.Render
{
    /// <summary>
    /// Maintains graphics information needed to render figures using OpenGL. A single renderer may only be used
    /// for one GL context.
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Sets up this renderer with the current graphics context.
        /// </summary>
        public void Initialize()
        {
            // Enable caps
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Creates a system typeface optimized (but not exclusive, or required) for this renderer.
        /// </summary>
        public SystemTypeface CreateSystemTypeface(string Name, bool Bold, bool Italic)
        {
            return new BitmapTypeface(Name, Bold, Italic);
        }

        /// <summary>
        /// Sets up the view on the current graphics context and renders a figure.
        /// </summary>
        public void Render(View View, int Width, int Height, bool InvertY, Figure Figure)
        {
            GL.Viewport(0, 0, Width, Height);

            double x = View.Offset.X;
            double y = View.Offset.Y;
            double a = View.Right.X;
            double b = View.Right.Y;
            double c = View.Down.X;
            double d = View.Down.Y;
            double i = InvertY ? -1.0 : 1.0;
            Matrix4d mat = new Matrix4d(
                0.5 * a, 0.5 * b, 0.0, 0.0,
                0.5 * c * i, 0.5 * d * i, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                x + 0.5 * (a + c), y + 0.5 * (b + d), 0.0, 1.0);
            mat.Invert();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mat);
            GL.CullFace(InvertY ? CullFaceMode.Front : CullFaceMode.Back);

            this._GetProcedure(Figure).Execute(new Context
            {
                Resolution = Width / View.Area * Height,
                Renderer = this
            });
        }

        /// <summary>
        /// Gets an unoptimized procedure that renders the given figure.
        /// </summary>
        private Procedure _GetProcedure(Figure Figure)
        {
            HintFigure hint = Figure as HintFigure;
            if (hint != null)
            {
                return this._GetProcedure(hint.Source);
            }

            TranslatedFigure translated = Figure as TranslatedFigure;
            if (translated != null)
            {
                return new TranslationProcedure(translated.Offset, this._GetProcedure(translated.Source));
            }

            SuperimposedFigure superimposed = Figure as SuperimposedFigure;
            if (superimposed != null)
            {
                return this._GetProcedure(superimposed.Under) + this._GetProcedure(superimposed.Over);
            }

            CompoundFigure compound = Figure as CompoundFigure;
            if (compound != null)
            {
                Procedure[] components = new Procedure[compound.Components.Count()];
                int i = 0;
                foreach (Figure component in compound.Components)
                {
                    components[i++] = this._GetProcedure(component);
                }
                return new CompoundProcedure(components);
            }

            ShapeFigure shape = Figure as ShapeFigure;
            if (shape != null)
            {
                return this._GetShapeProcedure(shape.Shape, shape.Source);
            }

            SystemFontGlyph sfg = Figure as SystemFontGlyph;
            if (sfg != null)
            {
                SystemFont font = sfg.Font;
                BitmapTypeface typeface = font.Typeface as BitmapTypeface;
                if (typeface != null)
                {
                    Texture tex;
                    Rectangle src;
                    Rectangle dst;
                    typeface.GetGlyph(sfg.Character).GetRenderInfo(font.Size, out tex, out src, out dst);
                    return
                        new BindTextureProcedure(tex) +
                        new SetColorProcedure(font.Color) +
                        new RenderGeometryProcedure(BeginMode.Quads, new BufferGeometry(
                            new Point[] { dst.TopLeft, dst.TopRight, dst.BottomRight, dst.BottomLeft },
                            null,
                            new Point[] { src.TopLeft, src.TopRight, src.BottomRight, src.BottomLeft }));
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a procedure that renders a figure confined to a certain shape.
        /// </summary>
        private Procedure _GetShapeProcedure(Shape Shape, Figure Source)
        {
            SolidFigure solidsource = Source as SolidFigure;
            if (solidsource != null)
            {
                Color fillcol = solidsource.Color;

                RectangleShape rectangle = Shape as RectangleShape;
                if (rectangle != null)
                {
                    Rectangle rect = rectangle.Rectangle;
                    return
                        BindTextureProcedure.Null +
                        new SetColorProcedure(fillcol) +
                        new RenderGeometryProcedure(BeginMode.Quads,
                            new BufferGeometry(new Point[] { rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft }, null, null));
                }

                PathShape pathshape = Shape as PathShape;
                if (pathshape != null)
                {
                    return
                        BindTextureProcedure.Null +
                        new SetColorProcedure(fillcol) +
                        this._GetPathProcedure(pathshape.Thickness, pathshape.Path);
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a procedure that renders the given path with the given thickness.
        /// </summary>
        private Procedure _GetPathProcedure(double Thickness, Graphics.Path Path)
        {
            // Not yet
            return null;
        }
    }
}