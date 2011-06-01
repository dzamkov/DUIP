using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that takes space with displaying any content.
    /// </summary>
    public class SpaceControl : Control
    {
        private SpaceControl()
        {

        }

        /// <summary>
        /// The only instance of this control.
        /// </summary>
        public static SpaceControl Singleton = new SpaceControl();

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            Size = SizeRange.TopLeft;
            return _Layout.Singleton;
        }

        private class _Layout : Layout
        {
            /// <summary>
            /// The only instance of the layout.
            /// </summary>
            public static readonly _Layout Singleton = new _Layout();

            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                
            }

            public override void Render(RenderContext Context)
            {
                
            }
        }
    }
}