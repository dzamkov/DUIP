using System;
using System.Collections.Generic;
using System.Linq;


namespace DUIP.UI
{
    /// <summary>
    /// A user-controlled pointlike object that can be used to manipulate nodes and objects in a world.
    /// </summary>
    public interface IProbe
    {
        /// <summary>
        /// Gets the position of the probe in the world.
        /// </summary>
        Point Position { get; }

        /// <summary>
        /// Gets if the probe is active.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Focuses the probe, allowing the listener to receive fine input from the probe.
        /// </summary>
        void Focus(IFocusListener Listener);
    }
    
    /// <summary>
    /// A dynamic collection of probes available for use.
    /// </summary>
    public interface IProbePool
    {
        /// <summary>
        /// Gets the probes in this pool.
        /// </summary>
        IEnumerable<IProbe> Probes { get; }

        /// <summary>
        /// Marks the given probe as used, preventing it from showing up in the probe pool until the next frame.
        /// </summary>
        void Use(IProbe Probe);

        /// <summary>
        /// Marks the given probe as locked, preventing it from showing up in the probe pool until the returned action is called.
        /// </summary>
        Action Lock(IProbe Probe);
    }

    /// <summary>
    /// A listener from events from a focused probe.
    /// </summary>
    public interface IFocusListener
    {
        /// <summary>
        /// Called when the probe typed the given character.
        /// </summary>
        void Type(char Character);

        /// <summary>
        /// Called when focus is lost for the associated probe. After this is called, the listener will not receive any more events.
        /// </summary>
        void Drop();
    }
}