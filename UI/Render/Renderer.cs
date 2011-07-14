using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI.Render
{
    /// <summary>
    /// Maintains graphics information needed to render figures using OpenGL.
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
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Get line width range
            Vector2 range;
            GL.GetFloat(GetPName.LineWidthRange, out range);
            this._MinLineWidth = range.X;
            this._MaxLineWidth = range.Y;
        }

        /// <summary>
        /// Sets up the view on the current graphics context and renders a figure.
        /// </summary>
        public void Render(View View, int Width, int Height, bool InvertY, Figure Figure)
        {
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


            this.Render(new RenderInfo
            {
                Resolution = res,
                Proportional = prop
            }, Figure);
        }

        /// <summary>
        /// Renders the given figure using the current graphics context.
        /// </summary>
        public void Render(RenderInfo Info, Figure Figure)
        {
            HintFigure hint = Figure as HintFigure;
            if (hint != null)
            {
                this.Render(Info, hint.Source);
                return;
            }

            TranslatedFigure translated = Figure as TranslatedFigure;
            if (translated != null)
            {
                Point offset = translated.Offset;
                GL.Translate(offset.X, offset.Y, 0.0);
                this.Render(Info, translated.Source);
                GL.Translate(-offset.X, -offset.Y, 0.0);
                return;
            }

            SuperimposedFigure superimposed = Figure as SuperimposedFigure;
            if (superimposed != null)
            {
                this.Render(Info, superimposed.Under);
                this.Render(Info, superimposed.Over);
                return;
            }

            CombinedFigure combined = Figure as CombinedFigure;
            if (combined != null)
            {
                this.Render(Info, combined.A);
                this.Render(Info, combined.B);
                return;
            }

            ShapeFigure shape = Figure as ShapeFigure;
            if (shape != null)
            {
                this.RenderShape(Info, shape.Shape, shape.Source);
                return;
            }

            TextureFigure texture = Figure as TextureFigure;
            if (texture != null)
            {
                Rectangle src = texture.Source;
                Rectangle dst = texture.Destination;
                texture.Texture.Bind();
                GL.Begin(BeginMode.Quads);
                double sl = src.Left;
                double st = src.Top;
                double sr = src.Right;
                double sb = src.Bottom;
                double dl = dst.Left;
                double dt = dst.Top;
                double dr = dst.Right;
                double db = dst.Bottom;
                GL.Color4(texture.Color);
                GL.TexCoord2(sl, st); GL.Vertex2(dl, dt);
                GL.TexCoord2(sr, st); GL.Vertex2(dr, dt);
                GL.TexCoord2(sr, sb); GL.Vertex2(dr, db);
                GL.TexCoord2(sl, sb); GL.Vertex2(dl, db);
                GL.End();
                return;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders a figure confined to a certain shape.
        /// </summary>
        public void RenderShape(RenderInfo Info, Shape Shape, Figure Source)
        {
            SolidFigure solidsource = Source as SolidFigure;
            if (solidsource != null)
            {
                Color fillcol = solidsource.Color;

                RectangleShape rectangle = Shape as RectangleShape;
                if (rectangle != null)
                {
                    Rectangle rect = rectangle.Rectangle;
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.Begin(BeginMode.Quads);
                    double l = rect.Left;
                    double t = rect.Top;
                    double r = rect.Right;
                    double b = rect.Bottom;
                    GL.Color4(fillcol);
                    GL.Vertex2(l, t);
                    GL.Vertex2(r, t);
                    GL.Vertex2(r, b);
                    GL.Vertex2(l, b);
                    GL.End();
                    return;
                }

                PathShape pathshape = Shape as PathShape;
                if (pathshape != null)
                {
                    this.RenderPath(Info, fillcol, pathshape.Thickness, pathshape.Path);
                    return;
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders a solid color path with a certain thickness.
        /// </summary>
        public void RenderPath(RenderInfo Info, Color Color, double Thickness, Graphics.Path Path)
        {
            // Not gonna bother with this for now
        }

        private double _MinLineWidth;
        private double _MaxLineWidth;
    }

    /// <summary>
    /// Contains information for a rendering.
    /// </summary>
    public class RenderInfo
    {
        /// <summary>
        /// The resolution for the rendering in pixels per unit.
        /// </summary>
        public double Resolution;

        /// <summary>
        /// Indicates wether both axies of the viewport are proportional with the view.
        /// </summary>
        public bool Proportional;
    }
}