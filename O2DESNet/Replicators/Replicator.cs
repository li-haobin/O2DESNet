using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace O2DESNet.Replicators
{
    public class Replicator<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public List<TScenario> Scenarios { get; private set; }
        public Dictionary<TScenario, RunningStatistics> Statistics { get; private set; }
        public long TotalBudget { get { return Statistics.Sum(i => i.Value.Count); } }
        public bool Parallelized { get; set; }
        protected Action<TScenario, int> Evaluate { get; private set; }

        public Replicator(
            IEnumerable<TScenario> scenarios, 
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double> objective,
            bool parallelized = true)
        {
            Scenarios = new List<TScenario>();
            Statistics = new Dictionary<TScenario, RunningStatistics>();
            Parallelized = parallelized;
            Evaluate = (scenario, seed) =>
            { 
                var simulator = constrSimulator(constrStatus(scenario, seed));
                while (!terminate(simulator.Status)) simulator.Run(1);
                Statistics[scenario].Push(objective(simulator.Status));
            };
            AddRange(scenarios);
        }
        public void Add(TScenario scenario) { AddRange(new TScenario[] { scenario }); }
        public void AddRange(IEnumerable<TScenario> scenarios)
        {
            Scenarios.AddRange(scenarios);
            foreach (var sc in scenarios) Statistics.Add(sc, new RunningStatistics());
            Alloc(scenarios.ToDictionary(sc => sc, sc => 2));
        }

        protected void Alloc(int budget, Dictionary<TScenario, double> targetRatio)
        {
            var asgmnt = targetRatio.Keys.ToDictionary(sc => sc, sc => 0);
            Func<TScenario, double> calPriority = sc => (Statistics[sc].Count + asgmnt[sc]) / targetRatio[sc];
            var priorities = targetRatio.Keys.ToDictionary(sc => sc, sc => calPriority(sc));
            var scenarios = targetRatio.Keys.OrderBy(sc => priorities[sc]).ToList();
            while (budget > 0)
            {
                // popup the first scenario
                var sc = scenarios.First();
                scenarios.RemoveAt(0);
                // assign budget
                asgmnt[sc]++; budget--;
                // update priority
                priorities[sc] = calPriority(sc);
                // re-order
                int i = 0;
                while (i < scenarios.Count && priorities[sc] > priorities[scenarios[i]]) i++;
                scenarios.Insert(i, sc);
            }
            Alloc(asgmnt);
        }
        protected void Alloc(Dictionary<TScenario, int> asgmnt)
        {
            var counts = asgmnt.Keys.ToDictionary(sc => sc, sc => (int)Statistics[sc].Count);
            if (Parallelized) Parallel.ForEach(asgmnt.Keys, sc => { Parallel.For(0, asgmnt[sc], i => { Evaluate(sc, counts[sc] + i); }); });
            else foreach (var sc in asgmnt.Keys) for (int i = 0; i < asgmnt[sc]; i++) Evaluate(sc, counts[sc] + i);
        }

        public void EqualAlloc(int budget) { Alloc(budget, Scenarios.ToDictionary(sc => sc, sc => 1.0)); }
    }
}
