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
        /// Determines wether the probe can be used by the given object in the current frame. Once a probe is used in a frame,
        /// it may not be used again until the next frame.
        /// </summary>
        public abstract bool Use(object Object);

        /// <summary>
        /// Locks the probe, making it unavailable for use by any object other than the one that is currently using it.
        /// </summary>
        public abstract void Lock();

        /// <summary>
        /// Releases a lock on the probe by the object currently using it.
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// Gets the position of the probe in the world.
        /// </summary>
        public abstract Point Position { get; }
    }
}