﻿using System;
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
        /// Indicates wether the current output buffer is proportional (the resolution
        /// on both axies is the same).
        /// </summary>
        public bool Proportional;

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