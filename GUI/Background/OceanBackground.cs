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
                    Figure.Solid(Color.RGB(0.5, 0.7, 0.9)),
                    1.0, 10.0,
                    CellularFigure.GridDistribution(Random, 6, 0.7, 1.0)), 
                new Rectangle(0.0, 0.0, 1.0, 1.0), 128, 128);
        }

        public override void Render(World World, View View)
        {
            // Background gradient
            double l = View.Area.Left;
            double t = View.Area.Top;
            double r = View.Area.Right;
            double b = View.Area.Bottom;
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.RGB(0.3, 0.6, 0.7));
            GL.Vertex2(l, t);
            GL.Vertex2(r, t);
            GL.Color3(Color.RGB(0.1, 0.4, 0.6));
            GL.Vertex2(r, b);
            GL.Vertex2(l, b);
            GL.End();
            GL.Enable(EnableCap.Texture2D);

            // Ripples
            this._Tex.Bind();
            GL.Color3(Color.White);
            Texture.DrawQuad(View.Area, View.Area);
        }

        public override void Update(World World, double Time)
        {

        }

        private Texture _Tex;
    }
}