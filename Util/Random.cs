using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DUIP
{
    /// <summary>
    /// A random number generator.
    /// </summary>
    /// <remarks>The RNG may access external information when creating random numbers. RNG's should be thread
    /// safe.</remarks>
    public abstract class Random
    {
        /// <summary>
        /// Gets a random signed integer with uniform distribution.
        /// </summary>
        public abstract int Integer();

        /// <summary>
        /// Gets a random double between 0.0 and 1.0 with uniform distribution.
        /// </summary>
        public abstract double Sample();

        /// <summary>
        /// Gets a version of this RNG to be used by only one thread. The returned RNG can not be passed
        /// between threads, but is usually more efficent when many random numbers are needed.
        /// </summary>
        public virtual Disposable<Random> Lock()
        {
            return this;
        }

        /// <summary>
        /// Gets the default RNG.
        /// </summary>
        public static Random Default
        {
            get
            {
                return _Default;
            }
        }

        private static Random _Default = new NativeRandom();

        public static implicit operator Random(System.Random Source)
        {
            return new NativeRandom(Source);
        }
    }

    /// <summary>
    /// A RNG using a native .net Random object.
    /// </summary>
    public sealed class NativeRandom : Random
    {
        public NativeRandom(System.Random Source)
        {
            this._Source = Source;
        }

        public NativeRandom(int Seed)
            : this(new System.Random(Seed))
        {

        }

        public NativeRandom()
            : this(new System.Random())
        {

        }

        /// <summary>
        /// Gets the source for this RNG.
        /// </summary>
        public System.Random Source
        {
            get
            {
                return this._Source;
            }
        }

        public override int Integer()
        {
            lock (this._Source)
            {
                return Integer(this._Source);
            }
        }

        /// <summary>
        /// Gets a random integer using the given random source.
        /// </summary>
        public static int Integer(System.Random Source)
        {
            byte[] buf = new byte[4];
            Source.NextBytes(buf);
            return BitConverter.ToInt32(buf, 0);
        }

        public override double Sample()
        {
            lock (this._Source)
            {
                return Sample(this._Source);
            }
        }

        /// <summary>
        /// Gets a random sample using the given random source.
        /// </summary>
        public static double Sample(System.Random Source)
        {
            return Source.NextDouble();
        }

        public override Disposable<Random> Lock()
        {
            Monitor.Enter(this._Source);
            return new _Locked(this._Source);
        }

        private sealed class _Locked : Random, IDisposable
        {
            public _Locked(System.Random Source)
            {
                this._Source = Source;
            }

            public override int Integer()
            {
                return NativeRandom.Integer(this._Source);
            }

            public override double Sample()
            {
                return NativeRandom.Sample(this._Source);
            }

            public void Dispose()
            {
                Monitor.Exit(this._Source);
            }

            private System.Random _Source;
        }

        public static implicit operator System.Random(NativeRandom Random)
        {
            return Random.Source;
        }

        private System.Random _Source;
    }
}