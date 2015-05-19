using BulkDeliver.Model;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Optimizer
{
    public class Selection
    {
        public Dictionary<Decision, RunningStatistics> Statistics;
        private Scenario _baseScenario;
        private Func<Scenario, int, double> _evaluate;
        public Decision[] Optima { get; private set; }
        public Selection(Scenario baseScenario, Func<Scenario, int, double> evaluate, params Decision[] decisions)
        {
            Statistics = new Dictionary<Decision, RunningStatistics>();
            _baseScenario = baseScenario;
            _evaluate = evaluate;
            foreach (var decision in decisions)
            {
                var scenario = decision.GetScenario(baseScenario);
                Statistics.Add(decision, new RunningStatistics(new double[] { evaluate(scenario, 0) }));
            }
        }
        public void Evaluate(double confidenceLevel, int maxNReplications, bool showProgress)
        {
            ShowProgress = () =>
            {
                if (showProgress)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("{0}/{1} Reps.", Statistics.Sum(s => s.Value.Count), maxNReplications);
                }
                return 0;
            };
            Optima = Statistics.Keys.ToArray();
            double z = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, confidenceLevel);
            while (Statistics.Values.Sum(v => v.Count) < maxNReplications && Optima.Count() > 1) Iterate(z);
        }
        public void Display()
        {
            foreach (var i in Statistics)
            {
                Console.WriteLine("{0} => {1:C}, {2:C} [{3} Reps.] {4}", i.Key, i.Value.Mean, i.Value.StandardDeviation, i.Value.Count, Optima.Contains(i.Key) ? "*" : "");
            }
        }

        private void Iterate(double z)
        {
            Parallel.ForEach(Optima, decision => { Evaluate(decision); });
            var min = Optima.OrderBy(d => Statistics[d].Mean).First();
            double upperbound = Statistics[min].Mean + z * Statistics[min].StandardDeviation / Math.Sqrt(1.0 * Statistics[min].Count);
            Optima = Statistics.Where(s => s.Value.Mean - z * s.Value.StandardDeviation / Math.Sqrt(1.0 * s.Value.Count) <= upperbound).Select(i => i.Key).ToArray();
        }

        private void Evaluate(Decision decision)
        {
            
            var stats = Statistics[decision];
            stats.Push(_evaluate(decision.GetScenario(_baseScenario), (int)stats.Count));
        }
        private Func<int> ShowProgress;
    }
}
