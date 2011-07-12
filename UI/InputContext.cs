using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A persistent context that UI elements can use to receive input from the user and other events.
    /// </summary>
    public abstract class InputContext
    {
        /// <summary>
        /// Gets or sets the function called when a signal for a probe changes while the probe is available to the context.
        /// wether
        /// </summary>
        public abstract ProbeSignalChangeHandler ProbeSignalChange { get; set; }

        /// <summary>
        /// Gets the probes available to this context.
        /// </summary>
        public abstract IEnumerable<Probe> Probes { get; }

        /// <summary>
        /// Registers a callback to be called periodically with the time in seconds since the last update.
        /// </summary>
        public abstract RemoveHandler RegisterUpdate(Action<double> Callback); 
    }

    /// <summary>
    /// Handler for a probe signal change for an input context. The handler may be null, in which case, it will work the same as
    /// a handler that returns false.
    /// </summary>
    /// <param name="Probe">The probe for which the signal has changed.</param>
    /// <param name="Signal">The signal of the probe that has changed.</param>
    /// <param name="Value">The new value of the signal.</param>
    /// <returns>Indicates wether the event was handled. If not, it may continue on to other handlers.</returns>
    public delegate bool ProbeSignalChangeHandler(Probe Probe, ProbeSignal Signal, bool Value);

    /// <summary>
    /// A user-controlled pointlike object that can be used to manipulate nodes and objects in a world.
    /// </summary>
    public abstract class Probe
    {
        /// <summary>
        /// Gets the position of the probe in relation to the context it was accessed from.
        /// </summary>
        public abstract Point Position { get; }

        /// <summary>
        /// Gets the current value of a signal produced by the probe.
        /// </summary>
        public abstract bool this[ProbeSignal Signal] { get; }

        /// <summary>
        /// Gets exclusive access to this probe. A locked probe will not create events in any input context and will not
        /// show up in the Probes collection. The probe can be released (unlocked) by calling the function returned by this
        /// method.
        /// </summary>
        public abstract Action Lock();

        /// <summary>
        /// Gets exclusive access to the fine input produced by this probe. The given function will be called when focus
        /// is lost.
        /// </summary>
        public abstract void Focus(Action Lost);

        /// <summary>
        /// Registers a callback to be called when the value of a signal for this probe changes.
        /// </summary>
        public abstract RemoveHandler RegisterSignalChange(Action<ProbeSignal, bool> Callback);
    }

    /// <summary>
    /// Identifies a boolean signal produced by a probe.
    /// </summary>
    public enum ProbeSignal
    {
        Primary
    }
}