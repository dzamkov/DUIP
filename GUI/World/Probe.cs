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
        /// Gets if the probe is "pressing" under itself. Pressing can active and drag objects.
        /// </summary>
        public abstract bool Pressed { get; }

        /// <summary>
        /// Gets if the probe is unlocked and available for use by any object.
        /// </summary>
        public bool Free
        {
            get
            {
                return this.Owner == null;
            }
        }

        /// <summary>
        /// Gets the owner of the lock on the probe, or null if the probe is not locked.
        /// </summary>
        public abstract object Owner { get; }

        /// <summary>
        /// Locks the probe, making it unavailable for use by any object other than the one that locked it.
        /// </summary>
        public abstract void Lock(object Owner);

        /// <summary>
        /// Releases a lock on the probe by the given owner object.
        /// </summary>
        public abstract void Release(object Owner);

        /// <summary>
        /// Gets the position of the probe in the world.
        /// </summary>
        public abstract Point Position { get; }
    }
}