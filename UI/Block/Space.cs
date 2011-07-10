using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A b;pcl that takes space without displaying any content.
    /// </summary>
    public class SpaceBlock : Block
    {
        private SpaceBlock()
        {

        }

        /// <summary>
        /// The only instance of this block.
        /// </summary>
        public static readonly SpaceBlock Instance = new SpaceBlock();

        /// <summary>
        /// Gets a layout that displays no content.
        /// </summary>
        public static Layout Layout
        {
            get
            {
                return _Layout.Instance;
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            Size = SizeRange.TopLeft;
            return _Layout.Instance;
        }

        private class _Layout : Layout
        {
            /// <summary>
            /// The only instance of the layout.
            /// </summary>
            public static readonly _Layout Instance = new _Layout();

            public override void Update(Point Offset, IEnumerable<Probe> Probes)
            {
                
            }

            public override void Render(RenderContext Context)
            {
                
            }
        }
    }
}