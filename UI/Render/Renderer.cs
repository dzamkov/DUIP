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
        /// Gets or sets the render cache for this renderer.
        /// </summary>
        public IRenderCache Cache
        {
            get
            {
                return this._Cache;
            }
            set
            {
                this._Cache = value;
            }
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

            this.GetProcedure(Figure, new Environment
            {

            }).Execute(new Context
            {
                InvertY = InvertY,
                InverseView = iview,
                Resolution = Width / View.Area * Height,
                Renderer = this,
                Modulation = Color.White
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
        /// Gets a procedure that renders the given figure with the given color modulation.
        /// </summary>
        public Procedure GetProcedure(Figure Figure, Environment Environment)
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
                CacheHintFigure cachehint = hint as CacheHintFigure;
                if (cachehint != null)
                {
                    IRenderCache cache = this._Cache;

                    Disposable<InStream> cacheread = cache.Read(cachehint.Name);
                    if (!cacheread.IsNull)
                    {
                        Environment.CacheRead = cacheread;
                        Procedure result = this.GetProcedure(hint.Source, Environment);
                        Environment.CacheRead = null;
                        cacheread.Dispose();
                        return result;
                    }

                    Disposable<OutStream> cachewrite = cache.Update(cachehint.Name);
                    if (!cachewrite.IsNull)
                    {
                        Environment.CacheWrite = cachewrite;
                        Procedure result = this.GetProcedure(hint.Source, Environment);
                        Environment.CacheWrite = null;
                        cachewrite.Dispose();
                        return result;
                    }
                }


                return this.GetProcedure(hint.Source, Environment);
            }

            // Translated figure
            TranslatedFigure translated = Figure as TranslatedFigure;
            if (translated != null)
            {
                return new ProjectionProcedure(View.Translation(translated.Offset), this.GetProcedure(translated.Source, Environment));
            }

            // Scaled figure
            ScaledFigure scaled = Figure as ScaledFigure;
            if (scaled != null)
            {
                return new ProjectionProcedure(View.Scale(scaled.Factor), this.GetProcedure(scaled.Source, Environment));
            }

            // Rotated figure
            RotatedFigure rotated = Figure as RotatedFigure;
            if (rotated != null)
            {
                return new ProjectionProcedure(View.Rotation(rotated.Angle), this.GetProcedure(rotated.Source, Environment));
            }

            // Projected figure
            ProjectedFigure projected = Figure as ProjectedFigure;
            if (projected != null)
            {
                return new ProjectionProcedure(projected.Projection, this.GetProcedure(projected.Source, Environment));
            }
            
            // Modulated figure
            ModulatedFigure modulated = Figure as ModulatedFigure;
            if (modulated != null)
            {
                return new ModulateProcedure(modulated.Modulation, this.GetProcedure(modulated.Source, Environment));
            }

            // Superimposed figure
            SuperimposedFigure superimposed = Figure as SuperimposedFigure;
            if (superimposed != null)
            {
                return this.GetProcedure(superimposed.Under, Environment) + this.GetProcedure(superimposed.Over, Environment);
            }

            // Compound figure
            CompoundFigure compound = Figure as CompoundFigure;
            if (compound != null)
            {
                Procedure[] components = new Procedure[compound.Components.Count()];
                int i = 0;
                foreach (Figure component in compound.Components)
                {
                    components[i++] = this.GetProcedure(component, Environment);
                }
                return new CompoundProcedure(components);
            }

            // Shape figure
            ShapeFigure shape = Figure as ShapeFigure;
            if (shape != null)
            {
                return this.GetShapeProcedure(shape.Shape, shape.Source, Environment);
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
                    int size = 256;
                    byte[] data = new byte[size * size * 4];
                    Texture tex;
                    unsafe
                    {
                        fixed (byte* ptr = data)
                        {
                            if (Environment.CacheRead != null)
                            {
                                Environment.CacheRead.Read(data, 0, data.Length);
                            }
                            else
                            {
                                Texture.WriteImageARGB(sampled, new View(Rectangle.UnitSquare), size, size, ptr);
                                if (Environment.CacheWrite != null)
                                {
                                    Environment.CacheWrite.Write(data, 0, data.Length);
                                }
                            }
                            tex = Texture.Create();
                            Texture.SetImage(Texture.Format.BGRA32, 0, size, size, ptr);
                            Texture.GenerateMipmap();
                            Texture.SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
                        }
                    }
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
        public Procedure GetShapeProcedure(Shape Shape, Figure Source, Environment Environment)
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
                        this.GetPathProcedure(pathshape.Thickness, pathshape.Path, Environment);
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a procedure that renders the given path with the given thickness.
        /// </summary>
        public Procedure GetPathProcedure(double Thickness, Graphics.Path Path, Environment Environment)
        {
            // Rectangle path
            RectanglePath rectangle = Path as RectanglePath;
            if (rectangle != null)
            {
                double ht = Thickness * 0.5;
                Rectangle rect = rectangle.Rectangle;
                return
                    new RenderGeometryProcedure(BeginMode.QuadStrip,
                        new BufferIndexedGeometry(
                            new BufferGeometry(new Point[] 
                            {
                                rect.TopLeft + new Point(ht, ht),
                                rect.TopLeft + new Point(-ht, -ht),
                                rect.TopRight + new Point(-ht, ht),
                                rect.TopRight + new Point(ht, -ht),
                                rect.BottomRight + new Point(-ht, -ht),
                                rect.BottomRight + new Point(ht, ht),
                                rect.BottomLeft + new Point(ht, -ht),
                                rect.BottomLeft + new Point(-ht, ht) 
                            }, null, null), new int[]
                            {
                                0, 1,
                                2, 3,
                                4, 5,
                                6, 7,
                                0, 1
                            }));
            }

            // Segment path
            SegmentPath segment = Path as SegmentPath;
            if (segment != null)
            {
                Point a = segment.A;
                Point b = segment.B;
                Point outward = (b - a).Perpendicular; outward *= Thickness * 0.5 / outward.Length;
                return
                    new RenderGeometryProcedure(BeginMode.Quads,
                        new BufferGeometry(new Point[]
                        {
                            a + outward,
                            a - outward,
                            b - outward,
                            b + outward
                        }, null, null));
            }


            // Not yet
            return null;
        }

        private IRenderCache _Cache;
        private Dictionary<Figure, Procedure> _Procedures;
    }

    /// <summary>
    /// Contains  information needed to create a procedure that renders a figure.
    /// </summary>
    public class Environment
    {
        /// <summary>
        /// The stream to use for reading cached procedure data, or null if no cached data is available.
        /// </summary>
        public InStream CacheRead;

        /// <summary>
        /// The stream to use for writing cached procedure data, or null if no cached data is to be written.
        /// </summary>
        public OutStream CacheWrite;
    }
}