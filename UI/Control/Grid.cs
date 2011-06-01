using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that displays a matrix of inner controls aligned in rows and columns.
    /// </summary>
    public class GridControl : Control
    {
        public GridControl()
        {

        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            throw new NotImplementedException();
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                throw new NotImplementedException();
            }

            public override void Render(RenderContext Context)
            {
                throw new NotImplementedException();
            }
        }
    }
}