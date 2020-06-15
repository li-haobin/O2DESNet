using Microsoft.VisualStudio.TestTools.UnitTesting;
using O2DESNet.RandomVariables.Discrete;
using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Discrete
{
    [TestClass]
    public class PoissonTests
    {
        [TestMethod]
        public void TestMeanAndVariacneConsistency()
        {
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Poisson poisson = new Poisson();
            rs.Clear();
            mean = 2000; stdev = Math.Sqrt(2000);
            poisson.Lambda = 2000;
            for (int i = 0; i < numSamples; ++i)
            {

                rs.Push(poisson.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("Poisson Discrete", mean, stdev * stdev, rs.Mean(), rs.Variance());
        }

    }
}
