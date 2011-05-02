using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.GUI
{
    /// <summary>
    /// A user-controlled pointlike object that can be used to manipulate nodes in a world. Probes may persist between frames.
    /// </summary>
    public abstract class Probe
    {
        /// <summary>
        /// Gets the "pressure" of the probe between 0.0 and 1.0. A probe's pressure relates to how strongly it drags, activates and
        /// holds objects. A probe with no pressure can not interact with the world.
        /// </summary>
        public abstract double Pressure { get; }

        /// <summary>
        /// Gets the position of the probe in the world.
        /// </summary>
        public abstract Point Position { get; }

        /// <summary>
        /// Gets the identity of this probe on the previous frame.
        /// </summary>
        public abstract Probe Previous { get; }
    }
}