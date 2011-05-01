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
            this._Tex = Texture.Create(
                new CellularFigure(
                    Figure.Solid(Color.Transparent),
                    Figure.Solid(Color.RGB(0.3, 1.0, 1.0)),
                    1.0, 8.0,
                    CellularFigure.GridDistribution(Random, 4, 0.7, 1.0)), 
                new Rectangle(0.0, 0.0, 1.0, 1.0), 128, 128);
        }

        public override void Render(View View)
        {
            GL.ClearColor(Color.RGB(0.2, 0.6, 0.8));
            GL.Clear(ClearBufferMask.ColorBufferBit);

            this._Tex.Bind();
            Texture.DrawQuad(View.Area, View.Area);
        }

        public override void Update(double Time)
        {

        }

        private Texture _Tex;
    }
}