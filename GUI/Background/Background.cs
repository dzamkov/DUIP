using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// A dynamic object responsible for rendering the background for a world. The main
    /// purpose of backgrounds is to allow the user to see when and how the camera moves when there are no 
    /// foreground objects.
    /// </summary>
    public abstract class Background
    {
        /// <summary>
        /// Renders the background to the current graphics context when the given view is used.
        /// </summary>
        public virtual void Render(World World, View View)
        {

        }

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
            /// Renders this layer to the given view.
            /// </summary>
            public void Render(View View)
            {
                this.Render(View, View.Zoom);   
                
            }

            /// <summary>
            /// Renders this layer to the given view with an alternate (or precomputed) zoom level.
            /// </summary>
            public void Render(View View, double Zoom)
            {
                double pg = (Zoom - this.MinZoom) / (this.MaxZoom - this.MinZoom);
                if (pg > 0.0 && pg < 1.0)
                {
                    double alpha = pg * (1.0 - pg) * 4.0;

                    this.Texture.Bind();
                    GL.Color4(Color.RGBA(1.0, 1.0, 1.0, alpha));

                    Rectangle src = View.Area;
                    double iscale = (1.0 / this.Scale);
                    src.TopLeft *= iscale;
                    src.BottomRight *= iscale;

                    Texture.DrawQuad(src, View.Area);
                }
            }

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