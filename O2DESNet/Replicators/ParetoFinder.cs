using MathNet.Numerics.Statistics;
using O2DESNet.MultiObjective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Replicators
{
    public class ParetoFinder<TScenario, TStatus, TSimulator> : Replicator<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public ParetoFinder(
            IEnumerable<TScenario> scenarios,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double[]> objectives, //double inDifferentZone = 0,
            bool inParallel = true
            ) :
            base(scenarios, constrStatus, constrSimulator, terminate, objectives, inParallel)
        { }

        public TScenario[] Optima
        {
            get
            {
                var nObjs = Objectives.Values.First().First().Length;
                return ParetoOptimality.GetParetoSet(Scenarios.ToArray(),
                    (s1, s2) => ParetoOptimality.Dominate(
                        Enumerable.Range(0, nObjs).Select(l => GetObjectives(s1, l).Mean()).ToArray(),
                        Enumerable.Range(0, nObjs).Select(l => GetObjectives(s2, l).Mean()).ToArray()));
            }
        }        
    }
}
