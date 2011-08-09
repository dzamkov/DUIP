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
            this.First = First;
            this.Second = Second;
        }

        /// <summary>
        /// The first component of the composite procedure.
        /// </summary>
        public readonly Procedure First;

        /// <summary>
        /// The second component of the composite procedure.
        /// </summary>
        public readonly Procedure Second;

        public override void Execute(Context Context)
        {
            this.First.Execute(Context);
            this.Second.Execute(Context);
        }
    }

    /// <summary>
    /// A procedure defined as the combination of multiple component procedures where the order of execution
    /// does not matter.
    /// </summary>
    public sealed class CompoundProcedure : Procedure
    {
        public CompoundProcedure(Procedure[] Components)
        {
            this.Components = Components;
        }

        /// <summary>
        /// The components of this procedure.
        /// </summary>
        public readonly Procedure[] Components;

        public override void Execute(Context Context)
        {
            for (int t = 0; t < this.Components.Length; t++)
            {
                this.Components[t].Execute(Context);
            }
        }
    }

    /// <summary>
    /// A procedure defined by two component procedures where either (but not both) may be executed.
    /// </summary>
    public sealed class DisjunctiveProcedure : Procedure
    {
        public DisjunctiveProcedure(Procedure A, Procedure B)
        {
            this.A = A;
            this.B = B;
        }

        /// <summary>
        /// The first component of the disjunctive procedure.
        /// </summary>
        public readonly Procedure A;

        /// <summary>
        /// The second component of the disjunctive procedure.
        /// </summary>
        public readonly Procedure B;

        public override void Execute(Context Context)
        {
            this.A.Execute(Context);
        }
    }

    /// <summary>
    /// A procedure that executes an inner procedure with a certain projection applied.
    /// </summary>
    public sealed class ProjectionProcedure : Procedure
    {
        public ProjectionProcedure(View Projection, Procedure Inner)
        {
            this.Inner = Inner;
            this.Projection = Projection;
        }

        /// <summary>
        /// The inner procedure for this procedure.
        /// </summary>
        public readonly Procedure Inner;

        /// <summary>
        /// The projection this procedure applies.
        /// </summary>
        public readonly View Projection;

        public override void Execute(Context Context)
        {
            View iview = Context.InverseView;
            View inneriview = View.Compose(this.Projection, iview);

            Renderer.UpdateProjection(Context.InvertY, inneriview);
            Context.InverseView = inneriview;
            this.Inner.Execute(Context);

            Context.InverseView = iview;
            Renderer.UpdateProjection(Context.InvertY, iview);
        }
    }

    /// <summary>
    /// A procedure that multiplies the colors of the inner procedure by a certain modulation color.
    /// </summary>
    public sealed class ModulateProcedure : Procedure
    {
        public ModulateProcedure(Color Modulation, Procedure Inner)
        {
            this.Modulation = Modulation;
            this.Inner = Inner;
        }

        /// <summary>
        /// The inner procedure for this procedure.
        /// </summary>
        public readonly Procedure Inner;

        /// <summary>
        /// The color of the modulation to apply.
        /// </summary>
        public readonly Color Modulation;

        public override void Execute(Context Context)
        {
            Color omod = Context.Modulation;
            Context.Modulation = omod * this.Modulation;
            this.Inner.Execute(Context);
            Context.Modulation = omod;
        } 
    }

    /// <summary>
    /// A procedure that renders some geometry using immediate mode.
    /// </summary>
    public sealed class RenderGeometryProcedure : Procedure
    {
        public RenderGeometryProcedure(BeginMode Mode, Geometry Geometry)
        {
            this.Mode = Mode;
            this.Geometry = Geometry;
        }

        /// <summary>
        /// The mode used to render the geometry.
        /// </summary>
        public readonly BeginMode Mode;

        /// <summary>
        /// The geometry to be rendered.
        /// </summary>
        public readonly Geometry Geometry;

        public override void Execute(Context Context)
        {
            GL.Begin(this.Mode);
            this.Geometry.Send();
            GL.End();
        }
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
            this.Color = Color;
        }

        /// <summary>
        /// A set color procedure that sets the current color to white.
        /// </summary>
        public static readonly SetColorProcedure White = new SetColorProcedure(Color.White);

        /// <summary>
        /// The color set by this procedure.
        /// </summary>
        public readonly Color Color;

        public override void Execute(Context Context)
        {
            GL.Color4(this.Color * Context.Modulation);
        }
    }

    /// <summary>
    /// A procedure that binds a certain texture.
    /// </summary>
    public sealed class BindTextureProcedure : Procedure
    {
        public BindTextureProcedure(Texture Texture)
        {
            this.Texture = Texture;
        }

        /// <summary>
        /// A bind texture procedure that binds the null texture.
        /// </summary>
        public static readonly BindTextureProcedure Null = new BindTextureProcedure(Texture.Null);

        /// <summary>
        /// Gets the texture to be bound.
        /// </summary>
        public readonly Texture Texture;

        public override void Execute(Context Context)
        {
            this.Texture.Bind();
        }
    }
}