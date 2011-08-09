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
            this.Positions = Positions;
            this.Colors = Colors;
            this.UVs = UVs;
        }

        /// <summary>
        /// The array for the positions of the vertices in this geometry.
        /// </summary>
        public readonly Point[] Positions;

        /// <summary>
        /// The array for the colors of the vertices in this geometry, or null if color information is not
        /// included.
        /// </summary>
        public readonly Color[] Colors;

        /// <summary>
        /// The array for the uv coordinates of the vertices in this geometry, or null if uv information is not
        /// included.
        /// </summary>
        public readonly Point[] UVs;

        public override int Size
        {
            get
            {
                return this.Positions.Length;
            }
        }

        public override VertexFormatFlags VertexFormat
        {
            get
            {
                VertexFormatFlags flags = VertexFormatFlags.None;
                if (this.Colors != null) flags |= VertexFormatFlags.Color;
                if (this.UVs != null) flags |= VertexFormatFlags.UV;
                return flags;
            }
        }

        public override void Send(int Index)
        {
            if (this.Colors != null)
            {
                GL.Color4(this.Colors[Index]);
            }
            if (this.UVs != null)
            {
                GL.TexCoord2((Vector2d)this.UVs[Index]);
            }
            GL.Vertex2((Vector2d)this.Positions[Index]);
        }
    }

    /// <summary>
    /// Indexed geometry with indices stored in an array.
    /// </summary>
    public sealed class BufferIndexedGeometry : IndexedGeometry
    {
        public BufferIndexedGeometry(Geometry Source, int[] Indices)
        {
            this._Source = Source;
            this.Indices = Indices;
        }

        /// <summary>
        /// The indices for this geometry.
        /// </summary>
        public readonly int[] Indices;

        public override int Size
        {
            get
            {
                return this.Indices.Length;
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
            return this.Indices[Index];
        }

        private Geometry _Source;
    }

    /// <summary>
    /// Geometry for the triangles defined in a mesh figure.
    /// </summary>
    public class MeshGeometry : IndexedGeometry
    {
        public MeshGeometry(MeshFigure Mesh)
        {
            this.Mesh = Mesh;
        }

        /// <summary>
        /// Gets the mesh this geometry is for.
        /// </summary>
        public readonly MeshFigure Mesh;

        public override int Size
        {
            get
            {
                return this.Mesh.Triangles.Length * 3;
            }
        }

        public override VertexFormatFlags VertexFormat
        {
            get
            {
                return VertexFormatFlags.Color;
            }
        }

        public override Geometry Source
        {
            get
            {
                return new MeshVertexGeometry(this.Mesh);
            }
        }

        public override int GetSourceIndex(int Index)
        {
            int triind = Index / 3;
            int cmpind = Index % 3;
            switch (cmpind)
            {
                case 0:
                    return this.Mesh.Triangles[triind].A;
                case 1:
                    return this.Mesh.Triangles[triind].B;
                default:
                    return this.Mesh.Triangles[triind].C;
            }
        }
    }

    /// <summary>
    /// Geometry for the vertices defined in a mesh figure.
    /// </summary>
    public class MeshVertexGeometry : Geometry
    {
        public MeshVertexGeometry(MeshFigure Mesh)
        {
            this.Mesh = Mesh;
        }

        /// <summary>
        /// The mesh this geometry is for.
        /// </summary>
        public readonly MeshFigure Mesh;

        public override int Size
        {
            get
            {
                return this.Mesh.Vertices.Length;
            }
        }

        public override VertexFormatFlags VertexFormat
        {
            get
            {
                return VertexFormatFlags.Color;
            }
        }

        public override void Send(int Index)
        {
            MeshVertex vert = this.Mesh.Vertices[Index];
            GL.Color4(vert.Color);
            GL.Vertex2((Vector2d)vert.Position);
        }
    }
}