using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
        /// Renders the background using the given context.
        /// </summary>
        public virtual void Render(World World, RenderContext Context)
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
            /// Renders this layer to the given context.
            /// </summary>
            public void Render(RenderContext Context)
            {
                this.Render(Context, Context.View.Zoom);   
                
            }

            /// <summary>
            /// Renders this layer to the given context with an alternate (or precomputed) zoom level.
            /// </summary>
            public void Render(RenderContext Context, double Zoom)
            {
                double pg = (Zoom - this.MinZoom) / (this.MaxZoom - this.MinZoom);
                if (pg > 0.0 && pg < 1.0)
                {
                    double alpha = pg * (1.0 - pg) * 4.0;

                    View view = Context.View;
                    Rectangle src = view.Area;
                    double iscale = (1.0 / this.Scale);
                    src.TopLeft *= iscale;
                    src.BottomRight *= iscale;

                    Context.SetTexture(this.Texture);
                    Context.SetColor(Color.RGBA(1.0, 1.0, 1.0, alpha));
                    Context.DrawTexturedQuad(src, view.Area);
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