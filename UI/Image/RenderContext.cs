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
    /// Contains functions and methods for rendering without causing interference to other rendering operations.
    /// </summary>
    public class RenderContext
    {
        public RenderContext(View View, int Width, int Height, bool InvertY)
        {
            GL.Viewport(0, 0, Width, Height);
            Point size = View.Area.Size;
            double wres = Width / size.X;
            double hres = Height / size.Y;

            const double propthreshold = 0.0001;
            this._Resolution = Math.Sqrt(wres * hres);
            this._Proportional = Math.Abs(wres - this._Resolution) < propthreshold && Math.Abs(hres - this._Resolution) < propthreshold;


            (this._View = View).Setup(InvertY);
            GL.CullFace(InvertY ? CullFaceMode.Front : CullFaceMode.Back);
            this._Pop = new _PopOnDispose() { Context = this };
            this._Effects = new Stack<_Effect>();
        }

        /// <summary>
        /// Initializes the render context for the first time.
        /// </summary>
        public static void Initialize()
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
            _MinLineWidth = range.X;
            _MaxLineWidth = range.Y;
        }

        /// <summary>
        /// Gets the view for the current context.
        /// </summary>
        public View View
        {
            get
            {
                return this._View;
            }
        }

        /// <summary>
        /// Gets the current pixels per unit for the view of the render context.
        /// </summary>
        public double Resolution
        {
            get
            {
                return this._Resolution;
            }
        }

        /// <summary>
        /// Gets if the view of the render context is proportional (the amount of pixels per unit is the same
        /// on both axies).
        /// </summary>
        public bool Proportional
        {
            get
            {
                return this._Proportional;
            }
        }

        /// <summary>
        /// Applies a translation effect that translates all rendering operations by the given amount.
        /// </summary>
        public IDisposable Translate(Point Translation)
        {
            this._PushEffect(new _TranslateEffect
            {
                Translation = Translation
            });
            return this._Pop;
        }

        /// <summary>
        /// Applies a draw effect that allows quads to be drawn.
        /// </summary>
        public IDisposable DrawQuads()
        {
            return this.Draw(BeginMode.Quads);
        }

        /// <summary>
        /// Applies a draw effect that begins a manual drawing operation.
        /// </summary>
        public IDisposable Draw(BeginMode Mode)
        {
            this._PushEffect(new _DrawEffect
            {
                Mode = Mode
            });
            return this._Pop;
        }

        /// <summary>
        /// Applies a draw effect that allows lines (of the given thickness) to be drawn.
        /// </summary>
        public IDisposable DrawLines(double Thickness)
        {
            if (this._Proportional)
            {
                double width = this._Resolution * Thickness;
                if (width < _MaxLineWidth)
                {
                    this._FakeLines = false;
                    GL.LineWidth((float)width);
                    return this.Draw(BeginMode.Lines);
                }
            }

            // Imitate lines using quads
            this._FakeLines = true;
            this._LineThickness = Thickness;
            return this.Draw(BeginMode.Quads);
        }

        /// <summary>
        /// Draws a cubic bezier curve with the given thickness.
        /// </summary>
        public void DrawBezierCurve(Point A, Point B, Point C, Point D, double Thickness)
        {
            double width = this._Resolution * Thickness;
            double elen = (D - C).Length + (C - B).Length + (B - A).Length;
            int segments = (int)(Math.Sqrt(this._Resolution * elen) * 0.7) + 2;
            double delta = 1.0 / segments;

            if (this._Proportional && width < _MaxLineWidth)
            {
                // Draw using lines
                GL.LineWidth((float)width);
                GL.Begin(BeginMode.LineStrip);
                GL.Vertex2((Vector2d)A);
                for (int t = 1; t <= segments; t++)
                {
                    double param = delta * t;
                    GL.Vertex2((Vector2d)Curve.EvaluateBezier(ref A, ref B, ref C, ref D, param));
                }
                GL.Vertex2((Vector2d)D);
                GL.End();
                return;
            }

            // Draw using quads
            Point da = B - A;
            Point db = C - B;
            Point dc = D - C;
            double hthickness = Thickness * 0.5;
            GL.Begin(BeginMode.QuadStrip);
            _OutputCurveStop(A, da.Direction, hthickness);
            for (int t = 1; t <= segments; t++)
            {
                double param = delta * t;
                _OutputCurveStop(
                    Curve.EvaluateBezier(ref A, ref B, ref C, ref D, param), 
                    Curve.EvaluateBezier(ref da, ref db, ref dc, param).Direction, hthickness);
            }
            _OutputCurveStop(D, dc.Direction, hthickness);
            GL.End();
        }

        /// <summary>
        /// Output a stop of a curve being drawn with a quad strip.
        /// </summary>
        private static void _OutputCurveStop(Point Position, Point Direction, double HalfThickness)
        {
            Direction = Direction.Perpendicular;
            Direction *= HalfThickness;
            GL.Vertex2((Vector2d)(Position + Direction));
            GL.Vertex2((Vector2d)(Position - Direction));
        }

        /// <summary>
        /// Outputs a quad.
        /// </summary>
        public void OutputQuad(Rectangle Quad)
        {
            double l = Quad.Left;
            double t = Quad.Top;
            double r = Quad.Right;
            double b = Quad.Bottom;
            GL.Vertex2(l, t);
            GL.Vertex2(r, t);
            GL.Vertex2(r, b);
            GL.Vertex2(l, b);
        }

        /// <summary>
        /// Outputs a texture-mapped quad.
        /// </summary>
        public void OutputTexturedQuad(Rectangle Source, Rectangle Destination)
        {
            double sl = Source.Left;
            double st = Source.Top;
            double sr = Source.Right;
            double sb = Source.Bottom;
            double dl = Destination.Left;
            double dt = Destination.Top;
            double dr = Destination.Right;
            double db = Destination.Bottom;
            GL.TexCoord2(sl, st); GL.Vertex2(dl, dt);
            GL.TexCoord2(sr, st); GL.Vertex2(dr, dt);
            GL.TexCoord2(sr, sb); GL.Vertex2(dr, db);
            GL.TexCoord2(sl, sb); GL.Vertex2(dl, db);
        }

        /// <summary>
        /// Outputs a line with the given endpoints.
        /// </summary>
        public void OutputLine(Point Start, Point End)
        {
            if (this._FakeLines)
            {
                // Make lines using quads
                Point o = (End - Start).Perpendicular;
                o *= (this._LineThickness / o.Length) * 0.5;
                GL.Vertex2((Vector2d)(Start + o));
                GL.Vertex2((Vector2d)(Start - o));
                GL.Vertex2((Vector2d)(End - o));
                GL.Vertex2((Vector2d)(End + o));
            }
            else
            {
                GL.Vertex2((Vector2d)Start);
                GL.Vertex2((Vector2d)End);
            }
        }

        /// <summary>
        /// Outputs a vertex with the given parameters. This should only be used when a draw mode is manually specified.
        /// </summary>
        public void OutputVertex(Point Position, Point UV, Color Color)
        {
            GL.TexCoord2((Vector2d)UV);
            GL.Color4(Color);
            GL.Vertex2((Vector2)Position);
        }

        /// <summary>
        /// Outputs a vertex with the given parameters. This should only be used when a draw mode is manually specified.
        /// </summary>
        public void OutputVertex(Point Position, Point UV)
        {
            GL.TexCoord2((Vector2d)UV);
            GL.Vertex2((Vector2)Position);
        }

        /// <summary>
        /// Outputs a vertex with the given parameters. This should only be used when a draw mode is manually specified.
        /// </summary>
        public void OutputVertex(Point Position, Color Color)
        {
            GL.Color4(Color);
            GL.Vertex2((Vector2)Position);
        }

        /// <summary>
        /// Outputs a vertex with the given parameters. This should only be used when a draw mode is manually specified.
        /// </summary>
        public void OutputVertex(Point Position)
        {
            GL.Vertex2((Vector2)Position);
        }

        /// <summary>
        /// Draws a single quad.
        /// </summary>
        public void DrawQuad(Rectangle Quad)
        {
            GL.Begin(BeginMode.Quads);
            this.OutputQuad(Quad);
            GL.End();
        }

        /// <summary>
        /// Draws a single textured quad.
        /// </summary>
        public void DrawTexturedQuad(Rectangle Source, Rectangle Destination)
        {
            GL.Begin(BeginMode.Quads);
            this.OutputTexturedQuad(Source, Destination);
            GL.End();
        }

        /// <summary>
        /// Draws a single textured quad.
        /// </summary>
        public void DrawTexturedQuad(Rectangle Quad)
        {
            this.DrawTexturedQuad(Rectangle.UnitSquare, Quad);
        }

        /// <summary>
        /// Sets the color used for future drawing operations.
        /// </summary>
        public void SetColor(Color Color)
        {
            GL.Color4(Color);
        }

        /// <summary>
        /// Sets the texture to be used for future drawing operations.
        /// </summary>
        public void SetTexture(Texture Texture)
        {
            Texture.Bind();
        }

        /// <summary>
        /// Clears the currently-set texture allowing drawing operations of solid colors.
        /// </summary>
        public void ClearTexture()
        {
           GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Pushes and applies an effect to the context.
        /// </summary>
        private void _PushEffect(_Effect Effect)
        {
            Effect.Apply(this);
            this._Effects.Push(Effect);
        }

        /// <summary>
        /// Removes the most recently pushed effect on the context.
        /// </summary>
        private void _PopEffect()
        {
            _Effect e = this._Effects.Pop();
            e.Remove(this);
        }

        /// <summary>
        /// Does what it says.
        /// </summary>
        private class _PopOnDispose : IDisposable
        {
            public void Dispose()
            {
                Context._PopEffect();
            }

            public RenderContext Context;
        }

        /// <summary>
        /// Represents an effect on a render context that may be applied and removed.
        /// </summary>
        private abstract class _Effect
        {
            /// <summary>
            /// Applies this effect.
            /// </summary>
            public abstract void Apply(RenderContext Context);

            /// <summary>
            /// Removes the effect.
            /// </summary>
            public abstract void Remove(RenderContext Context);
        }

        /// <summary>
        /// An effect that translates the coordinate space used for rendering by some amount.
        /// </summary>
        private sealed class _TranslateEffect : _Effect
        {
            public override void Apply(RenderContext Context)
            {
                GL.Translate((Vector3d)this.Translation);
                Context._View.Area = Context._View.Area.Translate(this.Translation);
            }

            public override void Remove(RenderContext Context)
            {
                GL.Translate(-(Vector3d)this.Translation);
                Context._View.Area = Context._View.Area.Translate(-this.Translation);
            }

            public Point Translation;
        }

        /// <summary>
        /// An effect that starts a drawing operation.
        /// </summary>
        private sealed class _DrawEffect : _Effect
        {
            public override void Apply(RenderContext Context)
            {
                GL.Begin(this.Mode);
            }

            public override void Remove(RenderContext Context)
            {
                GL.End();
            }

            public BeginMode Mode;
        }

        private _PopOnDispose _Pop;
        private View _View;
        private Stack<_Effect> _Effects;

        private double _Resolution;
        private bool _Proportional;

        private static double _MaxLineWidth;
        private static double _MinLineWidth;
        private bool _FakeLines;
        private double _LineThickness;
    }
}