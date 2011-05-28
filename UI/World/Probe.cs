using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.UI
{
    /// <summary>
    /// A user-controlled pointlike object that can be used to manipulate nodes in a world. Probes may persist between frames.
    /// </summary>
    public abstract class Probe
    {
        /// <summary>
        /// Gets if the probe is active.
        /// </summary>
        public abstract bool Active { get; }

        /// <summary>
        /// Gets or sets the object that has locked the probe for exclusive use, or null if the probe is available for all objects.
        /// </summary>
        public abstract object Lock { get; set; }

        /// <summary>
        /// Determines wether the probe can be used by the given object in the current frame. Once a probe is used in a frame,
        /// it may not be used again until the next frame.
        /// </summary>
        public abstract bool Use(object Object);

        /// <summary>
        /// Gets the position of the probe in the world.
        /// </summary>
        public abstract Point Position { get; }
    }
}