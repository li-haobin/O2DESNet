using MathNet.Numerics.Statistics;
using O2DESNet.MultiObjective;
using O2DESNet.Replicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Benchmarks
{
    class TestProgram
    {
        static void Main(string[] args)
        {
            var rs = new Random(0);

            int nTrials = 100;
            int budgetPerIterate = 30;
            
            var thetas = Enumerable.Range(0, nTrials).Select(i => 
                GetZDTs(type: 1, dimension: 3, noiseLevel: 0.1, size: 10, rs: rs)).ToArray();
            var equal = thetas.Select(t => new ParetoFinder(t)).AsParallel().ToArray();
            var mocba = thetas.Select(t => new MOCBA(t)).AsParallel().ToArray();
            Console.WriteLine("Budget\tPCS_EA\tPCS_MOCBA");
            while (true)
            {
                var rate1 = 1.0 * equal.Count(f => NbrErrors(f).Sum() == 0) / nTrials;
                var rate2 = 1.0 * mocba.Count(f => NbrErrors(f).Sum() == 0) / nTrials;
                Console.WriteLine("{0}\t{1:F4}\t{2:F4}", equal.First().TotalBudget, rate1, rate2);
                Parallel.ForEach(equal, f => f.Alloc(budgetPerIterate));
                Parallel.ForEach(mocba, f => f.Alloc(budgetPerIterate));
                if (rate1 == 1 || rate2 == 1) Console.ReadKey();
            }
        }

        /// <summary>
        /// The simplified Pareto finder for benchmark tests.
        /// </summary>
        class ParetoFinder : ParetoFinder<Benchmark, Status, Simulator>
        {
            public ParetoFinder(Benchmark[] scenarios) : base(
                scenarios: scenarios,
                constrStatus: (scenario, seed) => new Status(scenario, seed),
                constrSimulator: status => new Simulator(status),
                terminate: status => true,
                objectives: status => status.Objectives,
                inParallel: false)
            { }
        }
        /// <summary>
        /// The simplified MOCBA for benchmark tests.
        /// </summary>
        class MOCBA : MOCBA<Benchmark, Status, Simulator>
        {
            public MOCBA(Benchmark[] scenarios) : base(
                scenarios: scenarios,
                constrStatus: (scenario, seed) => new Status(scenario, seed),
                constrSimulator: status => new Simulator(status),
                terminate: status => true,
                objectives: status => status.Objectives,
                inParallel: false)
            { }
        }
        /// <summary>
        /// Display objective values of Pareto finder
        /// </summary>
        void DisplayObjs(ParetoFinder<Benchmark, Status, Simulator> paretoFinder)
        {
            Console.Clear();
            foreach (var s in paretoFinder.Scenarios) Console.WriteLine("{0},{1} -> {2},{3}",
                s.CalObjectives()[0], s.CalObjectives()[1], paretoFinder.GetObjectives(s, 0).Mean(), paretoFinder.GetObjectives(s, 1).Mean());
        }

        /// <summary>
        /// Get number of Type I & II errors for the given Pareto finder.
        /// </summary>
        static int[] NbrErrors(ParetoFinder<Benchmark, Status, Simulator> paretoFinder)
        {
            var nObjs = paretoFinder.Scenarios.First().NObjectives;
            var trueSet = new HashSet<Benchmark>(ParetoOptimality.GetParetoSet(paretoFinder.Scenarios.ToArray(),
                (s1, s2) => ParetoOptimality.Dominate(s1.CalObjectives(), s2.CalObjectives())));
            var observed = new HashSet<Benchmark>(paretoFinder.Optima);
            int nTypeI = 0, nTypeII = 0;
            foreach (var s in trueSet) if (!observed.Contains(s)) nTypeI++; // false positive (reject wrongly)
            foreach (var s in observed) if (!trueSet.Contains(s)) nTypeII++; // false negative (failed to reject)
            return new int[] { nTypeI, nTypeII };
        }

        #region scenarios generators
        static ZDTx[] GetZDTs(int type, int dimension, double noiseLevel, int size, Random rs)
        {
            var noiseLevels = Enumerable.Range(0, 2).Select(j => noiseLevel).ToArray();
            var zdts = new List<ZDTx>();
            while (zdts.Count < size)
            {
                var decisions = Enumerable.Range(0, dimension).Select(j => rs.NextDouble()).ToArray();
                switch (type)
                {
                    case 1: zdts.Add(new ZDT1(decisions, noiseLevels)); break;
                    case 2: zdts.Add(new ZDT2(decisions, noiseLevels)); break;
                    case 3: zdts.Add(new ZDT3(decisions, noiseLevels)); break;
                    case 4: zdts.Add(new ZDT4(decisions, noiseLevels)); break;
                    case 6: zdts.Add(new ZDT6(decisions, noiseLevels)); break;
                    default: throw new Exception("The type does exist for ZDTs.");
                }
            }
            return zdts.ToArray();
        }

        static DTLZx[] GetDTLZs(int type, int dimension, int nObjectives, double noiseLevel, int size, Random rs)
        {
            var noiseLevels = Enumerable.Range(0, nObjectives).Select(j => noiseLevel).ToArray();            
            var dtlzs = new List<DTLZx>();
            while (dtlzs.Count < size)
            {
                var decisions = Enumerable.Range(0, dimension).Select(j => rs.NextDouble()).ToArray();
                switch (type)
                {
                    case 1: dtlzs.Add(new DTLZ1(decisions, noiseLevels)); break;
                    case 2: dtlzs.Add(new DTLZ2(decisions, noiseLevels)); break;
                    case 3: dtlzs.Add(new DTLZ3(decisions, noiseLevels)); break;
                    case 4: dtlzs.Add(new DTLZ4(decisions, noiseLevels)); break;
                    case 5: dtlzs.Add(new DTLZ5(decisions, noiseLevels)); break;
                    case 6: dtlzs.Add(new DTLZ5(decisions, noiseLevels)); break;
                    case 7: dtlzs.Add(new DTLZ5(decisions, noiseLevels)); break;
                    default: throw new Exception("The type does exist for DTLZs.");
                }
            }
            return dtlzs.ToArray();
        }
        #endregion
    }
}
