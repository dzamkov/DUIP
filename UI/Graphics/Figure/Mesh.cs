using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A collection of vertices containing color and position information and triangles which reference vertices and interpolate the colors
    /// between them.
    /// </summary>
    public sealed class MeshFigure : Figure
    {
        public MeshFigure(MeshVertex[] Vertices, MeshTriangle[] Triangles)
        {
            this.Vertices = Vertices;
            this.Triangles = Triangles;
        }

        /// <summary>
        /// Creates a quadrilateral mesh figure.
        /// </summary>
        public static MeshFigure CreateQuad(MeshVertex TopLeft, MeshVertex TopRight, MeshVertex BottomLeft, MeshVertex BottomRight)
        {
            return new MeshFigure(
                new MeshVertex[] { TopLeft, TopRight, BottomLeft, BottomRight },
                new MeshTriangle[] { new MeshTriangle(0, 1, 2), new MeshTriangle(3, 2, 1) });
        }

        /// <summary>
        /// The vertices for this mesh.
        /// </summary>
        public readonly MeshVertex[] Vertices;

        /// <summary>
        /// The triangles for this mesh.
        /// </summary>
        public readonly MeshTriangle[] Triangles;
    }

    /// <summary>
    /// Contains information for a vertex within a mesh.
    /// </summary>
    public struct MeshVertex
    {
        public MeshVertex(Point Position, Color Color)
        {
            this.Position = Position;
            this.Color = Color;
        }

        /// <summary>
        /// The position of this vertex.
        /// </summary>
        public Point Position;

        /// <summary>
        /// The color of this vertex.
        /// </summary>
        public Color Color;
    }

    /// <summary>
    /// Contains information for a triangle within a mesh. The vertices in a triangle must be specified in clockwise order.
    /// </summary>
    public struct MeshTriangle
    {
        public MeshTriangle(int A, int B, int C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        /// <summary>
        /// The index of the first vertex of this triangle.
        /// </summary>
        public int A;

        /// <summary>
        /// The index of the second vertex of this triangle.
        /// </summary>
        public int B;

        /// <summary>
        /// The third vertex of this triangle.
        /// </summary>
        public int C;
    }
}