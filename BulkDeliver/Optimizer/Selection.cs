﻿using BulkDeliver.Model;
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
        private Dictionary<Decision, RunningStatistics> _statistics;
        private Scenario _baseScenario;
        private Func<Scenario, int, double> _evaluate;
        public Decision[] Optima { get; private set; }
        public Selection(Scenario baseScenario, Func<Scenario, int, double> evaluate, params Decision[] decisions)
        {
            _statistics = new Dictionary<Decision, RunningStatistics>();
            _baseScenario = baseScenario;
            _evaluate = evaluate;
            foreach (var decision in decisions)
            {
                var scenario = decision.GetScenario(baseScenario);
                _statistics.Add(decision, new RunningStatistics(new double[] { evaluate(scenario, 0), evaluate(scenario, 1) }));
            }
        }
        public void Evaluate(double confidenceLevel, int maxNReplications, bool showProgress)
        {
            ShowProgress = () =>
            {
                if (showProgress)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("{0}/{1} Reps.", _statistics.Sum(s => s.Value.Count), maxNReplications);
                }
                return 0;
            };
            Decision[] results = _statistics.Keys.ToArray();
            double z = MathNet.Numerics.Distributions.Normal.InvCDF(0, 1, confidenceLevel);
            while (_statistics.Values.Sum(v => v.Count) < maxNReplications)
            {                
                results = Iterate(z);
                if (results.Count() < 2) break;
            }
            Optima = results;
        }
        public void Display()
        {
            foreach (var i in _statistics)
            {
                Console.WriteLine("{0} => {1:C}, {2:C} [{3} Reps.] {4}", i.Key, i.Value.Mean, i.Value.StandardDeviation, i.Value.Count, Optima.Contains(i.Key) ? "*" : "");
            }
        }

        private Decision[] Iterate(double z)
        {
            var min = _statistics.OrderBy(s => s.Value.Mean).First().Key;            
            double upperbound = _statistics[min].Mean + z * _statistics[min].StandardDeviation / Math.Sqrt(1.0 * _statistics[min].Count);
            var decisionsToEvaluate = _statistics.Where(s => s.Value.Mean - z * s.Value.StandardDeviation / Math.Sqrt(1.0 * s.Value.Count) <= upperbound).Select(i => i.Key).ToArray();
            Parallel.ForEach(decisionsToEvaluate, decision => { Evaluate(decision); });
            ShowProgress();
            return decisionsToEvaluate;
        }

        private void Evaluate(Decision decision)
        {
            
            var stats = _statistics[decision];
            stats.Push(_evaluate(decision.GetScenario(_baseScenario), (int)stats.Count));
        }
        private Func<int> ShowProgress;
    }
}
