using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
}