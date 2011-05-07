using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Contains relevant, immutable, external information used to display a control.
    /// </summary>
    public abstract class ControlEnvironment
    {

    }

    /// <summary>
    /// A dynamic, displayable, instance of a block within a world.
    /// </summary>
    public abstract class Control : Figure
    {
        /// <summary>
        /// Gets the current size of the control.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Gets or sets the environment used to display this control.
        /// </summary>
        public abstract ControlEnvironment Environment { get; set; }

        /// <summary>
        /// Updates the state of the control by the given amount of time.
        /// </summary>
        /// <param name="Offset">The offset of the control in relation to the world.</param>
        /// <param name="Probes">The probes in the world.</param>
        public abstract void Update(Point Offset, IEnumerable<Probe> Probes, double Time);

        /// <summary>
        /// Called when the control will no longer be used.
        /// </summary>
        public virtual void Finish()
        {

        }

        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle(Point.Origin, this.Size);
            }
        }
    }
}