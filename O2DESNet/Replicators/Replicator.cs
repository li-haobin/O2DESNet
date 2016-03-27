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
        protected Action<TScenario, int> Evaluate { get; private set; }
        internal int InitBudget { get; private set; }

        public Replicator(
            IEnumerable<TScenario> scenarios, 
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, decimal> objective)
        {
            Scenarios = new List<TScenario>();
            Statistics = new Dictionary<TScenario, RunningStatistics>();
            Evaluate = (scenario, seed) =>
            { 
                var simulator = constrSimulator(constrStatus(scenario, seed));
                while (!terminate(simulator.Status)) simulator.Run(1);
                Statistics[scenario].Push(objective(simulator.Status));
            };
            InitBudget = 3;
            AddRange(scenarios);
        }
        public void Add(TScenario scenario) { AddRange(new TScenario[] { scenario }); }
        public void AddRange(IEnumerable<TScenario> scenarios)
        {
            // in case scenario exists
            scenarios = scenarios.Except(Scenarios).ToList(); 
            Scenarios.AddRange(scenarios);
            // in case the statistics exists, e.g., the scenario was removed and added back
            scenarios = scenarios.Except(Statistics.Keys).ToList(); 
            foreach (var sc in scenarios) Statistics.Add(sc, new RunningStatistics());
            Alloc(scenarios.ToDictionary(sc => sc, sc => InitBudget));
        }
        public void Remove(TScenario scenario) { Scenarios.Remove(scenario); }

        protected void Alloc(int budget, Dictionary<TScenario, decimal> targetRatio)
        {
            var asgmnt = targetRatio.Keys.ToDictionary(sc => sc, sc => 0);
            Func<TScenario, decimal> calPriority = sc => (Statistics[sc].Count + asgmnt[sc]) / targetRatio[sc];
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
            while (asgmnt.Sum(i => i.Value) > 0)
            {
                var counts = asgmnt.Keys.ToDictionary(sc => sc, sc => (int)Statistics[sc].Count);
                Parallel.ForEach(asgmnt.Keys, sc => { Parallel.For(0, asgmnt[sc], i => { Evaluate(sc, counts[sc] + i); }); });
                //foreach (var sc in asgmnt.Keys) for (int i = 0; i < asgmnt[sc]; i++) Evaluate(sc, counts[sc] + i); // non-parallelized
                foreach (var sc in asgmnt.Keys.ToArray()) asgmnt[sc] -= (int)Statistics[sc].Count - counts[sc];
            }            
        }

        public void EqualAlloc(int budget) { Alloc(budget, Scenarios.ToDictionary(sc => sc, sc => 1.0)); }

        public virtual void Display()
        {
            Console.WriteLine("mean\tstddev\t#reps");
            foreach(var sc in Scenarios)
            {
                var stats = Statistics[sc];
                Console.WriteLine("{0:F4}\t{1:F4}\t{2}", stats.Mean, stats.StandardDeviation, stats.Count);
            }
            Console.WriteLine("------------------");
            Console.WriteLine("Total Budget:\t{0}", TotalBudget);
            Console.WriteLine("# Scenarios:\t{0}", Scenarios.Count);
        }
    }
}
