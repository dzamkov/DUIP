using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A background that somewhat resembles an ocean.
    /// </summary>
    public class OceanAmbience : Ambience
    {
        public OceanAmbience(Random Random)
        {
            // Create layers
            double scale = 4.0;
            double zoom = -2.0;
            double error = 0.7;
            double rot = 0.0;
            this._Layers = new List<Layer>();
            for (int t = 0; t < 6; t++)
            {
                this._Layers.Add(new Layer
                {
                    Figure = new CellularFigure(
                        Color.Transparent, Color.RGB(0.5, 0.7, 0.9),
                        1.0, 40.0, CellularFigure.GridDistribution(Random, 12, error, 1.0)).Rotate(rot).Scale(scale),
                    MinZoom = zoom - 4.0,
                    MaxZoom = zoom + 8.0
                });
                scale *= 6.0;
                zoom += 1.6;
                error += 0.05;
                rot += 0.6;
            }
        }

        public override Figure GetScene(Figure Foreground, Camera Camera, View View)
        {
            double zoom = Camera.Zoom;

            Figure fig = null;
            foreach (Layer l in this._Layers)
            {
                double pg = (zoom - l.MinZoom) / (l.MaxZoom - l.MinZoom);
                if (pg > 0.0 && pg < 1.0)
                {
                    double op = pg * (1.0 - pg) * 4.0;
                    fig += l.Figure.Modulate(Color.RGBA(1.0, 1.0, 1.0, op));
                }
            }
            fig += Foreground;
            return fig;
        }

        /// <summary>
        /// Gives information about a layer of ocean background.
        /// </summary>
        public struct Layer
        {
            /// <summary>
            /// The figure used to display the layer with full opacity.
            /// </summary>
            public Figure Figure;

            /// <summary>
            /// The minimum zoom level this layer is visible on.
            /// </summary>
            public double MinZoom;

            /// <summary>
            /// The maximum zoom level this layer is visible on.
            /// </summary>
            public double MaxZoom;
        }

        private List<Layer> _Layers;
    }
}