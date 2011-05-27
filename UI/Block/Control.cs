using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A dynamic, displayable, instance of a block within a world.
    /// </summary>
    public abstract class Control : Visual
    {
        public sealed override Disposable<Visual> Update(Node Node, IEnumerable<Probe> Probes, double Time)
        {
            this.Update(Node.Position, Probes, Time);
            return this;
        }

        /// <summary>
        /// Sets the theme to be used by the control.
        /// </summary>
        public virtual Theme Theme
        {
            set
            {
                return;
            }
        }

        /// <summary>
        /// Sets the allowable size range for the control. Each point in the rectangle given represents a valid size.
        /// </summary>
        public virtual Rectangle SizeRange
        {
            set
            {
                return;
            }
        }

        /// <summary>
        /// Updates the state of the control by the given amount of time while receiving input from probes.
        /// </summary>
        /// <param name="Offset">The offset of the control from the probes.</param>
        /// <param name="Probes">The probes that affect the control.</param>
        public virtual void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {

        }
    }
}