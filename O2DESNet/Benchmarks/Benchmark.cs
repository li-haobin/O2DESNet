using System;
using System.Linq;

namespace O2DESNet.Benchmarks
{
    public abstract class Benchmark : Scenario
    {
        public int Dimension { get; protected set; }
        public int NumObjectives { get; protected set; }
        public double[] Decisions { get; private set; }
        /// <summary>
        /// standard deviation of noise term for all objectives
        /// </summary>
        public double[] NoiseLevels { get; private set; }
        public Benchmark(double[] decisions, double[] noiseLevels)
        {
            Decisions = decisions;
            NoiseLevels = noiseLevels;
            Dimension = Decisions.Length;
        }
        /// <summary>
        /// Calculate objective values from decisions without noise
        /// </summary>
        public abstract double[] CalObjectives();
        /// <summary>
        /// Generate noises according to specified levels, i.e., standard deviations
        /// </summary>
        internal virtual double[] GenNoises(Random rs)
        {
            return Enumerable.Range(0, NumObjectives).Select(l => MathNet.Numerics.Distributions.Normal.Sample(rs, 0, NoiseLevels[l])).ToArray();
        }
        internal virtual double[][] CalGradients()
        {
            return Enumerable.Range(0, NumObjectives).Select(l => Enumerable.Range(0, Dimension).Select(i => 0.0).ToArray()).ToArray();
        }
    }

    public class Status : Status<Benchmark>
    {
        /// <summary>
        /// number of dummy events executed
        /// </summary>
        public int RunLength { get; internal set; }
        public Status(Benchmark scenario, int seed) : base(scenario, seed)
        {
            Seed = new Optimizers.ArrayKey<double>(Scenario.Decisions).GetHashCode() + seed; // prevent CRN (common random number)
            var objs = scenario.CalObjectives();
            var noises = scenario.GenNoises(DefaultRS);
            Objectives = Enumerable.Range(0, objs.Length).Select(i => objs[i] + noises[i]).ToArray();
            RunLength = 0;
        }
        public double[] Objectives { get; private set; }
    }

    public class Simulator : Simulator<Benchmark, Status>
    {
        public Simulator(Status status) : base(status) { Schedule(new Count(), ClockTime); }
        class Count : Event<Benchmark, Status>
        {
            protected internal override void Invoke()
            {
                Status.RunLength++;
                Schedule(new Count(), ClockTime);
            }
        }
    }
}