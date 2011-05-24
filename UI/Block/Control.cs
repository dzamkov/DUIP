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
        public ControlEnvironment()
        {

        }

        public ControlEnvironment(ControlEnvironment Source)
        {
            this.Borders = Source.Borders;
            this.SizeRange = Source.SizeRange;
        }

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
    public abstract class Control : Content
    {
        /// <summary>
        /// Mutates this control, or creates an entirely new control for the block and the given parameters.
        /// </summary>
        public virtual Disposable<Control> Replace(Disposable<Control> Current, Block Block, ControlEnvironment Environment)
        {
            Current.Dispose();
            return Block.CreateControl(Environment);
        }

        public sealed override Disposable<Content> Update(Node Node, IEnumerable<Probe> Probes, double Time)
        {
            return (Control)this.Update(Node.Position, Probes, Time);
        }

        /// <summary>
        /// Updates the state of the control by the given amount of time while receiving input from probes. Returns the new state of the control. If
        /// interface for the control changes, the older interface should be disposed.
        /// </summary>
        /// <param name="Offset">The offset of the control from the probes.</param>
        /// <param name="Probes">The probes that affect the control.</param>
        public virtual Disposable<Control> Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            return this;
        }
    }
}