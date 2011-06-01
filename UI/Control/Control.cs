using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A dynamic, visual object users can interact with.
    /// </summary>
    /// <remarks>Controls are not displayed and updated directly, but rather must use a layout,
    /// which is a spatial arrangement of elements within a control.</remarks>
    public abstract class Control
    {
        /// <summary>
        /// Creates a layout for this control with the preferred size within the given size range. Layouts may be invalidated
        /// with changes to the control they are for, including changes caused by their own "Update" method.
        /// </summary>
        public abstract Layout CreateLayout(Rectangle SizeRange, out Point Size);

        /// <summary>
        /// A particular spatial arrangement of elements within a control.
        /// </summary>
        public abstract class Layout
        {
            /// <summary>
            /// Updates the state of the control this layout is for by the given amount of time while receiving input from probes. Note that this
            /// does not change the state of the layout itself.
            /// </summary>
            /// <param name="Offset">The offset of the control from the probes.</param>
            /// <param name="Probes">The probes that affect the control.</param>
            public virtual void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {

            }

            /// <summary>
            /// Renders the control (using the layout) to the given render context.
            /// </summary>
            public virtual void Render(RenderContext Context)
            {

            }
        }
    }
}