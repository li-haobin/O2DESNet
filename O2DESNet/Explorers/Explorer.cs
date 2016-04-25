﻿using O2DESNet.Replicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Explorers
{
    public class Explorer<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        protected DecisionSpace DecisionSpace { get; private set; }
        protected Func<double[], TScenario> ConstrScenario { get; private set; }
        protected bool Discrete { get; private set; }
        protected Random DefaultRS { get; private set; }
        /// <summary>
        /// lookup for scenario given decision values
        /// </summary>
        internal Dictionary<ArrayKey<double>, TScenario> Scenarios { get; private set; }
        /// <summary>
        /// lookup for decision given scenario
        /// </summary>
        internal Dictionary<TScenario, double[]> Decisions { get; private set; }
        public Replicator<TScenario, TStatus, TSimulator> Replicator { get; protected set; }

        public Explorer(
            DecisionSpace decisionSpace,
            Func<double[], TScenario> constrScenario,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double[]> objectives,
            bool discrete = false,
            int seed = 0)
        {
            DecisionSpace = decisionSpace;
            ConstrScenario = constrScenario;
            Discrete = discrete;
            DefaultRS = new Random(seed);
            Scenarios = new Dictionary<ArrayKey<double>, TScenario>();
            Decisions = new Dictionary<TScenario, double[]>();
        }

        public void Iterate(int sampleSize, int budget)
        {
            if (budget < sampleSize * Replicator.InitBudget) throw new Exception("Insufficient budget!");
            var decisions = Sample(sampleSize);
            int countNewScenarios = 0;
            for (int i = 0; i < decisions.Count; i++)
            {
                var decision = decisions[i];
                if (Discrete) decision = decision.Select(d => Math.Round(d)).ToArray(); // discretize
                var key = new ArrayKey<double>(decision);
                if (!Scenarios.ContainsKey(key))
                {
                    // create and include new scenario
                    var sc = ConstrScenario(decision);
                    Scenarios.Add(key, sc); Decisions.Add(sc, decision);
                    Replicator.Add(sc);
                    countNewScenarios++;
                }                
            }
            Replicator.Alloc(budget - countNewScenarios * Replicator.InitBudget);
        }
        protected virtual List<double[]> Sample(int size) { return DecisionSpace.Sample(size, DefaultRS); }
    }       
}