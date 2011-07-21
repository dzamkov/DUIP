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
        public Renderer()
        {
            this._Procedures = new Dictionary<Figure, Procedure>();
        }

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

            View iview = View.Inverse;
            GL.MatrixMode(MatrixMode.Projection);
            UpdateProjection(InvertY, iview);
            GL.CullFace(InvertY ? CullFaceMode.Front : CullFaceMode.Back);

            this._GetProcedure(Figure).Execute(new Context
            {
                InvertY = InvertY,
                InverseView = iview,
                Resolution = Width / View.Area * Height,
                Renderer = this
            });
        }

        /// <summary>
        /// Updates the projection on the current graphics context.
        /// </summary>
        public static void UpdateProjection(bool InvertY, View InverseView)
        {
            View v = InverseView;
            double x = v.Offset.X;
            double y = v.Offset.Y;
            double a = v.Right.X;
            double b = v.Right.Y;
            double c = v.Down.X;
            double d = v.Down.Y;
            double i = InvertY ? -1.0 : 1.0;
            Matrix4d mat = new Matrix4d(
                2.0 * a, 2.0 * b * i, 0.0, 0.0,
                2.0 * c, 2.0 * d * i, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                2.0 * x - 1.0, 2.0 * y * i - i, 0.0, 1.0);
            GL.LoadMatrix(ref mat);
        }

        /// <summary>
        /// Gets a procedure that renders the given figure.
        /// </summary>
        private Procedure _GetProcedure(Figure Figure)
        {
            // See if there is a stored procedure for it
            Procedure res;
            if(this._Procedures.TryGetValue(Figure, out res))
            {
                return res;
            }

            // Hint figure
            HintFigure hint = Figure as HintFigure;
            if (hint != null)
            {
                return this._GetProcedure(hint.Source);
            }

            // Translated figure
            TranslatedFigure translated = Figure as TranslatedFigure;
            if (translated != null)
            {
                return new ProjectionProcedure(View.Translation(translated.Offset), this._GetProcedure(translated.Source));
            }

            // Projected figure
            ProjectedFigure projected = Figure as ProjectedFigure;
            if (projected != null)
            {
                return new ProjectionProcedure(projected.Projection, this._GetProcedure(projected.Source));
            }

            // Superimposed figure
            SuperimposedFigure superimposed = Figure as SuperimposedFigure;
            if (superimposed != null)
            {
                return this._GetProcedure(superimposed.Under) + this._GetProcedure(superimposed.Over);
            }

            // Compound figure
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

            // Shape figure
            ShapeFigure shape = Figure as ShapeFigure;
            if (shape != null)
            {
                return this._GetShapeProcedure(shape.Shape, shape.Source);
            }

            // System font glyph
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

            // Sampled figure
            SampledFigure sampled = Figure as SampledFigure;
            if(sampled != null)
            {
                if(sampled.Tiled)
                {
                    Texture tex = Texture.Create(sampled, new View(Rectangle.UnitSquare), Texture.Format.BGRA32, 256, 256); 
                    Procedure proc = 
                        new BindTextureProcedure(tex) +
                        SetColorProcedure.White +
                        RenderViewProcedure.Singleton;
                    this._Procedures[sampled] = proc;
                    return proc;
                }
            }

            // No rendering method available
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

        private Dictionary<Figure, Procedure> _Procedures;
    }
}