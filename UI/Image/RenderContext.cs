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
        public RenderContext(View View)
        {
            (this._View = View).Setup();
            this._Pop = new _PopOnDispose() { Context = this };
            this._Effects = new Stack<_Effect>();
            this._ColorModulation = Color.White;
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
            GL.CullFace(CullFaceMode.Front);
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
        /// Gets the current color modulation used in the context.
        /// </summary>
        public Color ColorModulation
        {
            get
            {
                return this._ColorModulation;
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
        /// Applies an effect that modulates the color of drawing operations.
        /// </summary>
        public IDisposable ModulateColor(Color Color)
        {
            this._PushEffect(new _ModulateEffect
            {
                Modulation = Color
            });
            return this._Pop;
        }

        /// <summary>
        /// Applies a draw effect that allows quads to be drawn.
        /// </summary>
        public IDisposable DrawQuads()
        {
            this._PushEffect(new _DrawEffect
            {
                Mode = BeginMode.Quads
            });
            return this._Pop;
        }

        /// <summary>
        /// Applies a draw effect that allows lines (of the given thickness) to be drawn.
        /// </summary>
        public IDisposable DrawLines(double Thickness)
        {
            double width = this._View.Resolution * Thickness;
            if (width > _MaxLineWidth)
            {
                this._FakeLines = true;
                this._LineThickness = Thickness;
                this._PushEffect(new _DrawEffect
                {
                    Mode = BeginMode.Quads
                });
            }
            else
            {
                this._FakeLines = false;
                GL.LineWidth((float)width);
                this._PushEffect(new _DrawEffect
                {
                    Mode = BeginMode.Lines
                });
            }
            return this._Pop;
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
            GL.Color4(Color * this._ColorModulation);
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

        /// <summary>
        /// An effect that modulates the colors of drawing operations.
        /// </summary>
        private sealed class _ModulateEffect : _Effect
        {
            public override void Apply(RenderContext Context)
            {
                this.Old = Context._ColorModulation;
                Context._ColorModulation *= Modulation;    
            }

            public override void Remove(RenderContext Context)
            {
                Context._ColorModulation = this.Old;
            }

            public Color Old;
            public Color Modulation;
        }

        private _PopOnDispose _Pop;
        private View _View;
        private Color _ColorModulation;
        private Stack<_Effect> _Effects;

        private static double _MaxLineWidth;
        private static double _MinLineWidth;
        private bool _FakeLines;
        private double _LineThickness;
    }
}