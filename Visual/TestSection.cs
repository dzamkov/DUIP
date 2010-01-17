//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

namespace DUIP.Visual
{
    /// <summary>
    /// Section that draws a black cross.
    /// </summary>
    public class TestSection : Section
    {
        protected override void OnCreate(Context Context)
        {
            this._Brush = Context.CreateSolidBrush(new Color(0, 0, 0, 255));
            this._Poly = new ComplexPolygon();
            Grid maingrid = Context.Grid;
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.1, 0.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.5, 0.4));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.9, 0.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(1.0, 0.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(1.0, 0.1));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.6, 0.5));
            this._Poly.PushPoint(maingrid.GetRelativePoint(1.0, 0.9));
            this._Poly.PushPoint(maingrid.GetRelativePoint(1.0, 1.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.9, 1.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.5, 0.6));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.1, 1.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.0, 1.0));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.0, 0.9));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.4, 0.5));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.0, 0.1));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.0, 0.0));
        }

        protected override void OnDraw(Context Context)
        {
            Context.Draw(this._Poly, null, this._Brush);
        }

        protected override void OnDestroy()
        {

        }

        /// <summary>
        /// Creates an environment of test sections with the specified parent sector.
        /// </summary>
        /// <param name="Sector">The sector to create the test sections in.</param>
        /// <param name="Level">Recurse level. Start with 0.</param>
        public static void CreateIntrestingEnvironment(Sector Sector, int Level)
        {
            double hc = (double)Sector.GetHashCode();
            double ml = hc % (1.0 + ((double)Level * (double)Level));
            LVector size = Sector.Size;
            if (ml < 4.0)
            {
                for (int x = 0; x < size.Right; x++)
                {
                    for (int y = 0; y < size.Down; y++)
                    {
                        CreateIntrestingEnvironment(Sector.GetChild(new LVector(x, y)), Level + 1);
                    }
                }
            }
            else
            {
                new TestSection()._Add(Sector);
            }
        }

        private Brush _Brush;
        private ComplexPolygon _Poly;
    }
}
