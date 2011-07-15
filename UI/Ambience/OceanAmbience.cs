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
        /*public OceanAmbience(Random Random)
        {
            this._Layers = new List<Layer>();

            // Ripple layers
            double scale = 4.0;
            double zoom = -2.0;
            double error = 0.7;
            for (int t = 0; t < 6; t++)
            {
                this._Layers.Add(new Layer()
                {
                    Texture = Texture.CacheCreate<CellularImage>(
                        Program.Cache["ocean" + t.ToString() + ".png"],
                        delegate 
                        {
                            return new CellularImage(
                                Color.Transparent,
                                Color.RGB(0.5, 0.7, 0.9),
                                1.0, 40.0,
                                CellularImage.GridDistribution(Random, 12, error, 1.0));
                        },
                        new Rectangle(0.0, 0.0, 1.0, 1.0), 128, 128),
                    Scale = scale,
                    MinZoom = zoom - 4.0,
                    MaxZoom = zoom + 8.0
                });
                scale *= 6.0;
                zoom += 1.6;
                error += 0.05;
            }
        }*/

        public override void Update(World World, double Time)
        {

        }
    }
}