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
    }

    /// <summary>
    /// A dynamic, displayable, instance of a block within a world.
    /// </summary>
    public abstract class Control : Figure
    {
        /// <summary>
        /// Gets the current size of this control.
        /// </summary>
        public abstract Point Size { get; }

        /// <summary>
        /// Gets the prefered size of this control. The prefered size is a size that would
        /// better suit the control. The control is at its optimial size when the prefered size and
        /// actual size are equivalent. 
        /// </summary>
        public virtual Point PreferedSize
        {
            get
            {
                return this.Size;
            }
        }

        /// <summary>
        /// Mutates this control, or creates an entirely new control for the block and the given parameters.
        /// </summary>
        public virtual Control Replace(Block Block, Point Size, ControlEnvironment Environment)
        {
            this.Finish();
            return Block.CreateControl(Size, Environment);
        }

        /// <summary>
        /// Mutates this control, or creates an entirely new control to have the given size.
        /// </summary>
        public abstract Control Resize(Point Size);

        /// <summary>
        /// Updates the state of the control by the given amount of time.
        /// </summary>
        /// <param name="Offset">The offset of the control in relation to the world.</param>
        /// <param name="Probes">The probes in the world.</param>
        public virtual void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {

        }

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