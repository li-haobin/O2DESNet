using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Replicators
{
    public class MinSelector<TScenario, TStatus, TSimulator> : Replicator<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public MinSelector(
            IEnumerable<TScenario> scenarios,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double> objective,
            double inDifferentZone = 0,
            bool inParallel = true) :
            base(scenarios, constrStatus, constrSimulator, terminate, status => new double[] { objective(status) }, inParallel)
        { InDifferentZone = inDifferentZone; }

        private double _IDZConfidenceLevel = 0.99;
        public double InDifferentZone { get; private set; }
        public TScenario Optimum
        {
            get
            {
                return Objectives.Aggregate((s0, s1) => GetObjEvaluations(s0.Key, 0).Mean() <= GetObjEvaluations(s1.Key, 0).Mean() ? s0 : s1).Key;
            }
        }
        public TScenario[] InDifferentScenarios {
            get
            {
                var optimum = Optimum;
                return Scenarios.Where(sc => sc != optimum && ProbLessThan(
                    sc, Objectives[optimum].Select(o => o[0]).Mean(), Objectives[optimum].Select(o => o[0]).Variance() / Objectives[optimum].Count, InDifferentZone)
                    > _IDZConfidenceLevel).ToArray();
            }
        }

        private double ProbLessThan(TScenario sc, double mean, double sqrStdErr, double threshold)
        {
            var diff = GetObjEvaluations(sc, 0).Mean() - mean;
            var err = Math.Sqrt(GetObjEvaluations(sc, 0).Variance() / Objectives[sc].Count + sqrStdErr);
            return Normal.CDF(diff, err, threshold);
        }

        /// <summary>
        /// Probability of correct selection
        /// </summary>
        public double PCS {
            get
            {
                var optimum = Optimum;
                var minMean = GetObjEvaluations(optimum, 0).Mean();
                var sqrStdErr = GetObjEvaluations(optimum, 0).Variance() / Objectives[optimum].Count;
                double pcs = 1.0;
                foreach (var sc in Scenarios.Except(InDifferentScenarios)) if (sc != optimum)
                        pcs *= 1 - ProbLessThan(sc, minMean, sqrStdErr, 0);
                return pcs;
            }
        }
        
        public override void Display()
        {
            Scenarios.Sort((s1, s2) => GetObjEvaluations(s2, 0).Mean().CompareTo(GetObjEvaluations(s1, 0).Mean()));

            Console.WriteLine("mean\tstddev\t#reps");
            foreach (var sc in Scenarios)
            {
                var objectives = GetObjEvaluations(sc, 0);
                Console.Write("{0:F4}\t{1:F4}\t{2}\t", objectives.Mean(), objectives.StandardDeviation(), objectives.Count);
                if (sc == Optimum) Console.Write("*");
                if (InDifferentScenarios.Contains(sc)) Console.Write("-");
                Console.WriteLine();
            }
            Console.WriteLine("------------------");
            Console.WriteLine("Total Budget:\t{0}", TotalBudget);
            Console.WriteLine("# Scenarios:\t{0}", Scenarios.Count);

            Console.WriteLine("PCS:\t{0:F4}", PCS);
        }
    }
}
