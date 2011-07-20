using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI.Render
{
    /// <summary>
    /// Contains context information for use by a rendering operation.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// The current view of the world space that is being rendered.
        /// </summary>
        public View View;

        /// <summary>
        /// The resolution of the current output buffer in pixels per unit.
        /// </summary>
        public double Resolution;

        /// <summary>
        /// The renderer that is performing the rendering operation.
        /// </summary>
        public Renderer Renderer;
    }
}