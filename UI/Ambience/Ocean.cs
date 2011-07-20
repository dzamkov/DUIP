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
            this._CellularFigure = new CellularFigure(
                Color.Transparent, Color.RGB(0.5, 0.7, 0.9),
                1.0, 40.0,
                CellularFigure.GridDistribution(Random, 12, 0.7, 1.0));
        }

        public override Figure GetScene(Figure Foreground, Camera Camera, View View)
        {
            return this._CellularFigure + Foreground;
        }

        private CellularFigure _CellularFigure;
    }
}