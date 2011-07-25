using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI.Render
{
    /// <summary>
    /// A static collection of vertices.
    /// </summary>
    public abstract class Geometry
    {
        /// <summary>
        /// Gets the amount of vertices in the geometry set.
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// Gets the vertex format for vertices in the geometry set.
        /// </summary>
        public abstract VertexFormatFlags VertexFormat { get; }

        /// <summary>
        /// Sends the vertex with the given index to the current graphics context using immediate mode.
        /// </summary>
        public abstract void Send(int Index);

        /// <summary>
        /// Sends all the vertices in this geometry set to the current graphics context using imeediate mode.
        /// </summary>
        public void Send()
        {
            int size = this.Size;
            for (int t = 0; t < size; t++)
            {
                this.Send(t);
            }
        }
    }

    /// <summary>
    /// Geometry that takes vertices from some source geometry set and arranges them in an order defined by
    /// indices.
    /// </summary>
    public abstract class IndexedGeometry : Geometry
    {
        /// <summary>
        /// Gets the source geometry for this geometry. All indices in this geometry are references to vertices
        /// in the source geometry.
        /// </summary>
        public abstract Geometry Source { get; }

        /// <summary>
        /// Gets the index of the vertex in the source geometry to use for the vertex at the given index in
        /// this geometry.
        /// </summary>
        public abstract int GetSourceIndex(int Index);

        public override VertexFormatFlags VertexFormat
        {
            get
            {
                return this.Source.VertexFormat;
            }
        }

        public override void Send(int Index)
        {
            this.Source.Send(this.GetSourceIndex(Index));
        }
    }

    /// <summary>
    /// Identifies the contents of vertices within a geometry set.
    /// </summary>
    [Flags]
    public enum VertexFormatFlags
    {
        None = 0x00000000,

        /// <summary>
        /// Indicates that vertices contain color information.
        /// </summary>
        Color = 0x00000001,

        /// <summary>
        /// Indicates that vertices contain texture coordinates.
        /// </summary>
        UV = 0x00000002,
    }

    /// <summary>
    /// Geometry stored in arrays.
    /// </summary>
    public sealed class BufferGeometry : Geometry
    {
        public BufferGeometry(Point[] Positions, Color[] Colors, Point[] UVs)
        {
            this._Positions = Positions;
            this._Colors = Colors;
            this._UVs = UVs;
        }

        /// <summary>
        /// Gets the array for the positions of the vertices in this geometry.
        /// </summary>
        public Point[] Positions
        {
            get
            {
                return this._Positions;
            }
        }

        /// <summary>
        /// Gets the array for the colors of the vertices in this geometry, or null if color information is not
        /// included.
        /// </summary>
        public Color[] Colors
        {
            get
            {
                return this._Colors;
            }
        }

        /// <summary>
        /// Gets the array for the uv coordinates of the vertices in this geometry, or null if uv information is not
        /// included.
        /// </summary>
        public Point[] UVs
        {
            get
            {
                return this._UVs;
            }
        }

        public override int Size
        {
            get
            {
                return this._Positions.Length;
            }
        }

        public override VertexFormatFlags VertexFormat
        {
            get
            {
                VertexFormatFlags flags = VertexFormatFlags.None;
                if (this._Colors != null) flags |= VertexFormatFlags.Color;
                if (this._UVs != null) flags |= VertexFormatFlags.UV;
                return flags;
            }
        }

        public override void Send(int Index)
        {
            if (this._Colors != null)
            {
                GL.Color4(this._Colors[Index]);
            }
            if (this._UVs != null)
            {
                GL.TexCoord2((Vector2d)this._UVs[Index]);
            }
            GL.Vertex2((Vector2d)this._Positions[Index]);
        }

        private Point[] _Positions;
        private Color[] _Colors;
        private Point[] _UVs;
    }

    /// <summary>
    /// Indexed geometry with indices stored in an array.
    /// </summary>
    public sealed class BufferIndexedGeometry : IndexedGeometry
    {
        public BufferIndexedGeometry(Geometry Source, int[] Indices)
        {
            this._Source = Source;
            this._Indices = Indices;
        }

        /// <summary>
        /// Gets the indices for this geometry.
        /// </summary>
        public int[] Indices
        {
            get
            {
                return this._Indices;
            }
        }

        public override int Size
        {
            get
            {
                return this._Indices.Length;
            }
        }

        public override Geometry Source
        {
            get
            {
                return this._Source;
            }
        }

        public override int GetSourceIndex(int Index)
        {
            return this._Indices[Index];
        }

        private Geometry _Source;
        private int[] _Indices;
    }
}