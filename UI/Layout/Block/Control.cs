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
    }
}