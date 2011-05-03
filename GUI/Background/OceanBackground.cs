using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// A background that somewhat resembles an ocean.
    /// </summary>
    public class OceanBackground : Background
    {
        public OceanBackground(Random Random)
        {
            this._Layers = new List<Layer>();

            // Ripple layers
            double scale = 2.0;
            double zoom = -2.0;
            double error = 0.7;
            for (int t = 0; t < 6; t++)
            {
                
                this._Layers.Add(new Layer()
                {
                    Texture = Texture.Create(
                        new CellularFigure(
                            Figure.Solid(Color.Transparent),
                            Figure.Solid(Color.RGB(0.5, 0.7, 0.9)),
                            1.0, 7.0,
                            CellularFigure.GridDistribution(Random, 6, error, 1.0)),
                        new Rectangle(0.0, 0.0, 1.0, 1.0), 128, 128),
                    Scale = scale,
                    MinZoom = zoom - 4.0,
                    MaxZoom = zoom + 6.0
                });
                scale *= 6.0;
                zoom += 1.6;
                error += 0.05;
            }
        }

        public override void Render(World World, View View)
        {
            double zoom = View.Zoom;

            // Background gradient
            double wash = Math.Min(1.0, Math.Max(0.0, (zoom + 2.0) / 10.0));
            Color topcol = Color.Mix(Color.RGB(0.3, 0.6, 0.7), Color.RGB(0.3, 0.5, 0.8), wash);
            Color botcol = Color.Mix(Color.RGB(0.1, 0.4, 0.6), Color.RGB(0.1, 0.3, 0.7), wash);

            double l = View.Area.Left;
            double t = View.Area.Top;
            double r = View.Area.Right;
            double b = View.Area.Bottom;
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(BeginMode.Quads);
            GL.Color3(topcol);
            GL.Vertex2(l, t);
            GL.Vertex2(r, t);
            GL.Color3(botcol);
            GL.Vertex2(r, b);
            GL.Vertex2(l, b);
            GL.End();
            GL.Enable(EnableCap.Texture2D);

            // Layers
            foreach (Layer layer in this._Layers)
            {
                layer.Render(View, zoom);
            }
        }

        public override void Update(World World, double Time)
        {

        }

        private List<Layer> _Layers;
    }
}