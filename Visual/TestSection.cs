//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.2, 0.1));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.5, 0.4));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.8, 0.1));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.9, 0.2));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.6, 0.5));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.9, 0.8));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.8, 0.9));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.5, 0.6));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.2, 0.9));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.1, 0.8));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.4, 0.5));
            this._Poly.PushPoint(maingrid.GetRelativePoint(0.1, 0.2));
        }

        protected override void OnDraw(Context Context)
        {
            Context.Draw(this._Poly, null, this._Brush);
        }

        protected override void OnDestroy()
        {

        }

        private Brush _Brush;
        private ComplexPolygon _Poly;
    }
}
