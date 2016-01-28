using O2DESNet.Replicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Explorers
{
    public class RandomSearch<TScenario, TStatus, TSimulator> : Explorer<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public RandomSearch(
            DecisionSpace decisionSpace,
            Func<double[], TScenario> constrScenario,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double> objective,
            double inDifferentZone,
            bool discrete = false,
            int seed = 0) :
            base(decisionSpace, constrScenario, constrStatus, constrSimulator, terminate,
                status => new double[] { objective(status) }, discrete, seed)
        {
            Replicator = new MinSelector<TScenario, TStatus, TSimulator>(
                new TScenario[] { }, constrStatus, constrSimulator, terminate, objective, inDifferentZone);
        }

        public TScenario Optimum { get { return ((MinSelector<TScenario, TStatus, TSimulator>)Replicator).Optimum; } }
    }
}
