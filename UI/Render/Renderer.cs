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

            // Get line width range
            Vector2 range;
            GL.GetFloat(GetPName.LineWidthRange, out range);
            this._MinLineWidth = range.X;
            this._MaxLineWidth = range.Y;
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
            Point size = View.Area.Size;
            Point center = View.Area.TopLeft + size * 0.5;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Scale(2.0 / size.X, InvertY ? -2.0 / size.Y : 2.0 / size.Y, 1.0);
            GL.Translate(-center.X, -center.Y, 0.0);
            GL.CullFace(InvertY ? CullFaceMode.Front : CullFaceMode.Back);

            double wres = Width / size.X;
            double hres = Height / size.Y;
            double res = Math.Sqrt(wres * hres);

            const double propthreshold = 0.0001;
            bool prop = Math.Abs(wres - res) < propthreshold && Math.Abs(hres - res) < propthreshold;


            this._GetProcedure(Figure).Execute(new Context
            {
                Resolution = res,
                Proportional = prop,
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

        private double _MinLineWidth;
        private double _MaxLineWidth;
    }
}