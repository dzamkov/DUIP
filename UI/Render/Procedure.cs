using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI.Render
{
    /// <summary>
    /// Represents a rendering procedure that can be executed to render an object or
    /// change the state of the graphics context. A procedure with a value of null indicates
    /// that no action needs to be taken upon execution.
    /// </summary>
    public abstract class Procedure
    {
        /// <summary>
        /// Executes this procedure using the given context.
        /// </summary>
        public abstract void Execute(Context Context);

        public static Procedure operator +(Procedure A, Procedure B)
        {
            if (A == null)
                return B;
            if (B == null)
                return A;
            return new CompositeProcedure(A, B);
        }

        public static Procedure operator |(Procedure A, Procedure B)
        {
            if (A == null || B == null)
                return null;
            return new DisjunctiveProcedure(A, B);
        }
    }

    /// <summary>
    /// A procedure that is defined as the ordered combination of two component procedures.
    /// </summary>
    public sealed class CompositeProcedure : Procedure
    {
        public CompositeProcedure(Procedure First, Procedure Second)
        {
            this._First = First;
            this._Second = Second;
        }

        /// <summary>
        /// Gets the first component of the composite procedure.
        /// </summary>
        public Procedure First
        {
            get
            {
                return this._First;
            }
        }

        /// <summary>
        /// Gets the second component of the composite procedure.
        /// </summary>
        public Procedure Second
        {
            get
            {
                return this._Second;
            }
        }

        public override void Execute(Context Context)
        {
            this._First.Execute(Context);
            this._Second.Execute(Context);
        }

        private Procedure _First;
        private Procedure _Second;
    }

    /// <summary>
    /// A procedure defined as the combination of multiple component procedures where the order of execution
    /// does not matter.
    /// </summary>
    public sealed class CompoundProcedure : Procedure
    {
        public CompoundProcedure(Procedure[] Components)
        {
            this._Components = Components;
        }

        /// <summary>
        /// Gets the components of this procedure.
        /// </summary>
        public Procedure[] Components
        {
            get
            {
                return this._Components;
            }
        }

        public override void Execute(Context Context)
        {
            for (int t = 0; t < this._Components.Length; t++)
            {
                this._Components[t].Execute(Context);
            }
        }

        private Procedure[] _Components;
    }

    /// <summary>
    /// A procedure defined by two component procedures where either (but not both) may be executed.
    /// </summary>
    public sealed class DisjunctiveProcedure : Procedure
    {
        public DisjunctiveProcedure(Procedure A, Procedure B)
        {
            this._A = A;
            this._B = B;
        }

        /// <summary>
        /// Gets the first component of the disjunctive procedure.
        /// </summary>
        public Procedure A
        {
            get
            {
                return this._A;
            }
        }

        /// <summary>
        /// Gets the second component of the disjunctive procedure.
        /// </summary>
        public Procedure B
        {
            get
            {
                return this._B;
            }
        }

        public override void Execute(Context Context)
        {
            this._A.Execute(Context);
        }

        private Procedure _A;
        private Procedure _B;
    }

    /// <summary>
    /// A procedure that executes an inner procedure with a certain projection applied.
    /// </summary>
    public sealed class ProjectionProcedure : Procedure
    {
        public ProjectionProcedure(View Projection, Procedure Inner)
        {
            this._Inner = Inner;
            this._Projection = Projection;
        }

        /// <summary>
        /// Gets the inner procedure for this procedure.
        /// </summary>
        public Procedure Inner
        {
            get
            {
                return this._Inner;
            }
        }

        /// <summary>
        /// Gets the projection this procedure applies.
        /// </summary>
        public View Projection
        {
            get
            {
                return this._Projection;
            }
        }

        public override void Execute(Context Context)
        {
            View iview = Context.InverseView;
            View inneriview = View.Compose(this._Projection, iview);

            Renderer.UpdateProjection(Context.InvertY, inneriview);
            Context.InverseView = inneriview;
            this._Inner.Execute(Context);

            Context.InverseView = iview;
            Renderer.UpdateProjection(Context.InvertY, iview);
        }

        private Procedure _Inner;
        private View _Projection;
    }

    /// <summary>
    /// A procedure that multiplies the colors of the inner procedure by a certain modulation color.
    /// </summary>
    public sealed class ModulateProcedure : Procedure
    {
        public ModulateProcedure(Color Modulation, Procedure Inner)
        {
            this._Modulation = Modulation;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets the inner procedure for this procedure.
        /// </summary>
        public Procedure Inner
        {
            get
            {
                return this._Inner;
            }
        }

        /// <summary>
        /// Gets the color of the modulation to apply.
        /// </summary>
        public Color Modulation
        {
            get
            {
                return this._Modulation;
            }
        }

        public override void Execute(Context Context)
        {
            Color omod = Context.Modulation;
            Context.Modulation = omod * this._Modulation;
            this._Inner.Execute(Context);
            Context.Modulation = omod;
        }

        private Procedure _Inner;
        private Color _Modulation;
    }

    /// <summary>
    /// A procedure that renders some geometry using immediate mode.
    /// </summary>
    public sealed class RenderGeometryProcedure : Procedure
    {
        public RenderGeometryProcedure(BeginMode Mode, Geometry Geometry)
        {
            this._Mode = Mode;
            this._Geometry = Geometry;
        }

        /// <summary>
        /// Gets the mode used to render the geometry.
        /// </summary>
        public BeginMode Mode
        {
            get
            {
                return this.Mode;
            }
        }

        /// <summary>
        /// Gets the geometry to be rendered.
        /// </summary>
        public Geometry Geometry
        {
            get
            {
                return this.Geometry;
            }
        }

        public override void Execute(Context Context)
        {
            GL.Begin(this._Mode);
            this._Geometry.Send();
            GL.End();
        }

        private BeginMode _Mode;
        private Geometry _Geometry;
    }

    /// <summary>
    /// Renders a texture quad over the entire viewport, with texture coordinates corresponding to those at world space.
    /// </summary>
    public sealed class RenderViewProcedure : Procedure
    {
        private RenderViewProcedure()
        {

        }

        /// <summary>
        /// The only instance of this class.
        /// </summary>
        public static RenderViewProcedure Singleton = new RenderViewProcedure();

        public override void Execute(Context Context)
        {
            View view = Context.InverseView.Inverse;
            Point tl = view.TopLeft;
            Point tr = view.TopRight;
            Point bl = view.BottomLeft;
            Point br = view.BottomRight;
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2((Vector2)tl); GL.Vertex2((Vector2)tl);
            GL.TexCoord2((Vector2)tr); GL.Vertex2((Vector2)tr);
            GL.TexCoord2((Vector2)br); GL.Vertex2((Vector2)br);
            GL.TexCoord2((Vector2)bl); GL.Vertex2((Vector2)bl);
            GL.End();
        }
    }

    /// <summary>
    /// A procedure that sets the color to be used for future rendering operations. This takes into account the current
    /// color modulation set by the context.
    /// </summary>
    public sealed class SetColorProcedure : Procedure
    {
        public SetColorProcedure(Color Color)
        {
            this._Color = Color;
        }

        /// <summary>
        /// A set color procedure that sets the current color to white.
        /// </summary>
        public static readonly SetColorProcedure White = new SetColorProcedure(Color.White);

        /// <summary>
        /// Gets the color set by this procedure.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        public override void Execute(Context Context)
        {
            GL.Color4(this._Color * Context.Modulation);
        }

        private Color _Color;
    }

    /// <summary>
    /// A procedure that binds a certain texture.
    /// </summary>
    public sealed class BindTextureProcedure : Procedure
    {
        public BindTextureProcedure(Texture Texture)
        {
            this._Texture = Texture;
        }

        /// <summary>
        /// A bind texture procedure that binds the null texture.
        /// </summary>
        public static readonly BindTextureProcedure Null = new BindTextureProcedure(Texture.Null);

        /// <summary>
        /// Gets the texture to be bound.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return this._Texture;
            }
        }

        public override void Execute(Context Context)
        {
            this._Texture.Bind();
        }

        private Texture _Texture;
    }
}