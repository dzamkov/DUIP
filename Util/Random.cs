using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DUIP
{
    /// <summary>
    /// A random number generator.
    /// </summary>
    /// <remarks>The RNG may access external information when creating random numbers.</remarks>
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
            this.Source = Source;
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
        public readonly System.Random Source;

        public override int Integer()
        {
            byte[] buf = new byte[4];
            this.Source.NextBytes(buf);
            return BitConverter.ToInt32(buf, 0);
        }

        public override double Sample()
        {
            return this.Source.NextDouble();
        }

        public static implicit operator System.Random(NativeRandom Random)
        {
            return Random.Source;
        }
    }
}