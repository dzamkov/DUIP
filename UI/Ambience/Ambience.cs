using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A dynamic object responsible for rendering the background for a world. The main
    /// purpose of backgrounds is to allow the user to see when and how the camera moves when there are no 
    /// foreground objects.
    /// </summary>
    public abstract class Ambience
    {
        /// <summary>
        /// Updates the state of the background by the given amount of time in seconds.
        /// </summary>
        public virtual void Update(World World, double Time)
        {

        }

        /// <summary>
        /// A tiled, parallaxed level of a background.
        /// </summary>
        public struct Layer
        {
            /// <summary>
            /// Gets the texture source for the layer.
            /// </summary>
            public Texture Texture;

            /// <summary>
            /// The size of a tile of the texture when displayed.
            /// </summary>
            public double Scale;

            /// <summary>
            /// The minimum zoom level at which this layer is visible.
            /// </summary>
            public double MinZoom;

            /// <summary>
            /// The maximum zoom level at which this layer is visible.
            /// </summary>
            public double MaxZoom;
        }
    }
}