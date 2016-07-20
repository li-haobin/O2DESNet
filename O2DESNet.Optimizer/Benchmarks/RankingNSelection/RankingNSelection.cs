using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Benchmarks
{
    public abstract class RankingNSelection
    {
        public StochasticSolution[] Solutions { get; protected set; }
        public double[] TrueMeans { get; protected set; }
        public double[] TrueStdDevs { get; protected set; }
        protected Random _defaultRS { get; private set; }
        protected Random[] _randoms { get; private set; }

        protected RankingNSelection(int nDesigns, int seed)
        {
            Solutions = Enumerable.Range(0, nDesigns).Select(i => new StochasticSolution(new double[] { i })).ToArray();
            TrueMeans = new double[nDesigns];
            TrueStdDevs = new double[nDesigns];
            _defaultRS = new Random(seed);
            _randoms = Solutions.Select(s => new Random(_defaultRS.Next())).ToArray();
        }
        
        public void Evaluate(int index, int budget)
        {
            for (int i = 0; i < budget; i++)
                Solutions[index].Evaluate(new double[] { Normal.Sample(_randoms[index], TrueMeans[index], TrueStdDevs[index]) });
        } 
        
        /// <summary>
        /// Theoretical probability of correct selection
        /// </summary>
        public double PCS
        {
            get
            {
                var means = TrueMeans; //Solutions.Select(s => s.Objectives[0]).ToArray();
                var stddevs = TrueStdDevs; //Solutions.Select(s => s.StandardDeviations[0]).ToArray();
                var budgets = Solutions.Select(s => s.Observations.Count).ToArray();

                var minIndex = Enumerable.Range(0, Solutions.Length).Aggregate((i1, i2) => means[i1] < means[i2] ? i1 : i2);
                var pcs = 1.0;
                for (int i = 0; i < Solutions.Length; i++)
                    if (i != minIndex)
                        pcs *= Normal.CDF(means[minIndex] - means[i], Math.Sqrt(stddevs[i] * stddevs[i] / budgets[i] + stddevs[minIndex] * stddevs[minIndex] / budgets[minIndex]), 0);
                return pcs;
            }
        }

        /// <summary>
        /// Theoretical variance of observed minimum
        /// </summary>
        public double Variance
        {
            get
            {
                var rs = new Random(0);                
                return Enumerable.Range(0, 1000) // Monte Carlo sample size
                    .Select(k => Enumerable.Range(0, Solutions.Length).Min(i => 
                    Normal.Sample(rs, TrueMeans[i], TrueStdDevs[i] / Math.Sqrt(Solutions[i].Observations.Count))
                    )).Variance();
            }
        }
    }

    public class RnS_SlippageConfiguration : RankingNSelection
    {
        public RnS_SlippageConfiguration(int nDesigns, double rho, int seed = 0) : base(nDesigns, seed)
        {
            TrueMeans[0] = 1;
            TrueStdDevs[0] = 1;
            foreach(int i in Enumerable.Range(1, nDesigns - 1))
            {
                TrueMeans[i] = 2;
                TrueStdDevs[i] = 1 / Math.Sqrt(rho);
            }
        }
    }

    public class RnS_MonotoneDecreasingMeans : RankingNSelection
    {
        public RnS_MonotoneDecreasingMeans(int nDesigns, double vPower, int seed = 0) : base(nDesigns, seed)
        {
            double delta = 1;
            foreach (int i in Enumerable.Range(0, nDesigns))
            {
                TrueMeans[i] = i + 1;
                TrueStdDevs[i] = Math.Pow(Math.Abs(i + 1 - delta) + 1, vPower / 2);
            }
        }
    }
}
