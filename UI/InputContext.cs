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
        /// Gets the probes available to this context.
        /// </summary>
        public abstract IEnumerable<Probe> Probes { get; }

        /// <summary>
        /// Registers a callback to be called when the value for a signal of a probe available to the context changes.
        /// </summary>
        public abstract RemoveHandler RegisterProbeSignalChange(ProbeSignalChangeHandler Callback);

        /// <summary>
        /// Registers a callback to be called periodically with the time in seconds since the last update.
        /// </summary>
        public abstract RemoveHandler RegisterUpdate(Action<double> Callback);

        /// <summary>
        /// Creates a translated form of this input context.
        /// </summary>
        public virtual InputContext Translate(Point Offset)
        {
            return new TranslatedInputContext(Offset, this);
        }
    }

    /// <summary>
    /// Handler for a probe signal change for an input context.
    /// </summary>
    /// <param name="Probe">The probe for which the signal has changed.</param>
    /// <param name="Signal">The signal of the probe that has changed.</param>
    /// <param name="Value">The new value of the signal.</param>
    /// <param name="Handled">Indicates wether the event was handled.</param>
    public delegate void ProbeSignalChangeHandler(Probe Probe, ProbeSignal Signal, bool Value, ref bool Handled);

    /// <summary>
    /// An input context translated from a source input context.
    /// </summary>
    public class TranslatedInputContext : InputContext
    {
        public TranslatedInputContext(Point Offset, InputContext Source)
        {
            this._Offset = Offset;
            this._Source = Source;
        }

        /// <summary>
        /// Gets the offset of this input context from the source context.
        /// </summary>
        public Point Offset
        {
            get
            {
                return this._Offset;
            }
        }

        /// <summary>
        /// Gets the source input context for this context.
        /// </summary>
        public InputContext Source
        {
            get
            {
                return this._Source;
            }
        }

        public override IEnumerable<Probe> Probes
        {
            get
            {
                return
                    from probe in this._Source.Probes
                    select probe.Translate(this._Offset);
            }
        }

        public override RemoveHandler RegisterProbeSignalChange(ProbeSignalChangeHandler Callback)
        {
            return this._Source.RegisterProbeSignalChange(delegate(Probe Probe, ProbeSignal Signal, bool Value, ref bool Handled)
            {
                Callback(Probe.Translate(this._Offset), Signal, Value, ref Handled);
            });
        }

        public override RemoveHandler RegisterUpdate(Action<double> Callback)
        {
            return this._Source.RegisterUpdate(Callback);    
        }

        public override InputContext Translate(Point Offset)
        {
            return new TranslatedInputContext(this._Offset + Offset, this._Source);
        }

        private Point _Offset;
        private InputContext _Source;
    }

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

        /// <summary>
        /// Creates a probe with the given translation from this one.
        /// </summary>
        public virtual Probe Translate(Point Offset)
        {
            return new TranslatedProbe(Offset, this);
        }
    }

    /// <summary>
    /// A probe that acts as a decorated form of another probe.
    /// </summary>
    public class DerivedProbe : Probe
    {
        public DerivedProbe(Probe Source)
        {
            this._Source = Source;
        }

        /// <summary>
        /// Gets the source probe for this probe.
        /// </summary>
        public Probe Source
        {
            get
            {
                return this._Source;
            }
        }

        public override Point Position
        {
            get
            {
                return this._Source.Position;
            }
        }

        public override bool this[ProbeSignal Signal]
        {
            get
            {
                return this._Source[Signal];
            }
        }

        public override Action Lock()
        {
            return this._Source.Lock();
        }

        public override void Focus(Action Lost)
        {
            this._Source.Focus(Lost);
        }

        public override RemoveHandler RegisterSignalChange(Action<ProbeSignal, bool> Callback)
        {
            return this._Source.RegisterSignalChange(Callback);
        }

        private Probe _Source;
    }

    /// <summary>
    /// A probe that is translated some fixed amount from a source probe.
    /// </summary>
    public class TranslatedProbe : DerivedProbe
    {
        public TranslatedProbe(Point Offset, Probe Source)
            : base(Source)
        {
            this._Offset = Offset;
        }

        /// <summary>
        /// Gets the offset of this probe from the source probe.
        /// </summary>
        public Point Offset
        {
            get
            {
                return this._Offset;
            }
        }

        public override Point Position
        {
            get
            {
                return this.Source.Position + this._Offset;
            }
        }

        public override Probe Translate(Point Offset)
        {
            return new TranslatedProbe(this._Offset + Offset, this.Source);
        }

        private Point _Offset;
    }

    /// <summary>
    /// Identifies a boolean signal produced by a probe.
    /// </summary>
    public enum ProbeSignal
    {
        Primary
    }
}