using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Contains immutable external information used to display a control.
    /// </summary>
    public class ControlEnvironment
    {
        /// <summary>
        /// The external borders of the control.
        /// </summary>
        public Compass<Border> Borders;

        /// <summary>
        /// A rectangle containing the points that correspond to all valid sizes the control may have.
        /// </summary>
        public Rectangle SizeRange;
    }

    /// <summary>
    /// A dynamic, displayable, instance of a block within a world.
    /// </summary>
    public abstract class Control : Figure
    {
        /// <summary>
        /// Gets the static size of this control.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Mutates this control, or creates an entirely new control for the block and the given parameters.
        /// </summary>
        public virtual Disposable<Control> Replace(Block Block, ControlEnvironment Environment)
        {
            return Block.CreateControl(Environment);
        }

        /// <summary>
        /// Updates the state of the control by the given amount of time.
        /// </summary>
        /// <param name="Offset">The offset of the control in relation to the world.</param>
        /// <param name="Probes">The probes in the world.</param>
        public virtual void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
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