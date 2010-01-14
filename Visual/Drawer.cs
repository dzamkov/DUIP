using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DUIP.Visual
{
    /// <summary>
    /// Class used to draw a world with the view specified.
    /// </summary>
    public class Drawer
    {
        public Drawer()
        {

        }

        /// <summary>
        /// Renders the specified view on to GL.
        /// </summary>
        /// <param name="View">The view to render.</param>
        /// <param name="AspectRatio">Width divided by height of the viewport.</param>
        public void Render(View View, double AspectRatio)
        {
            // Clear
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0, 0, 0, 255);

            // Set ortho projection.
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            Matrix4d proj = this.GetProjectionMatrix(View, AspectRatio);
            GL.LoadMatrix(ref proj);
            

            // Set view projection
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Draw
            GL.Begin(BeginMode.Quads);
            GL.Color4(255.0, 255.0, 255.0, 255.0);
            GL.Vertex3(0.0, 0.0, -1.0);
            GL.Color4(255.0, 255.0, 255.0, 255.0);
            GL.Vertex3(1.0, 0.0, -1.0);
            GL.Color4(255.0, 0.0, 255.0, 255.0);
            GL.Vertex3(1.0, 1.0, -1.0);
            GL.Color4(255.0, 255.0, 255.0, 255.0);
            GL.Vertex3(0.0, 1.0, -1.0);
            GL.End();

            // Restore
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        /// <summary>
        /// Gets the projection matrix onto the the specified view.
        /// </summary>
        /// <param name="View">The view to create the projection matrix for.</param>
        /// <param name="AspectRatio">The aspect ratio of the viewport.</param>
        /// <returns>The projection matrix that can be used to project an
        /// object on a sector to the viewport.</returns>
        public Matrix4d GetProjectionMatrix(View View, double AspectRatio)
        {
            Matrix4d mat = Matrix4d.Identity;
            mat *= Matrix4d.CreateTranslation(-View.Location.Offset.Down, -View.Location.Offset.Right, 0.0);
            mat *= Matrix4d.Scale(View.ZoomLevel, View.ZoomLevel, 1.0);
            mat *= Matrix4d.Scale(0.5, -0.5, 1.0);
            if (AspectRatio > 1.0)
            {
                mat *= Matrix4d.CreateOrthographic(1.0, 1.0 / AspectRatio, -1.0, 1.0);
            }
            else
            {
                mat *= Matrix4d.CreateOrthographic(1.0 * AspectRatio, 1.0, -1.0, 1.0);
            }
            return mat;
        }
    }
}
