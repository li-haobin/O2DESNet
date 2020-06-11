using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using O2DESNet.RandomVariables.Continuous;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous
{
    [TestClass]
    public class GammaTests
    {
        [TestMethod]
        public void TestMeanAndVariacneConsistency()
        {
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Gamma gamma = new Gamma();
            rs.Clear();
            mean = 2; stdev = 5;
            for (int i = 0; i < numSamples; ++i)
            {
                gamma.Mean = mean;
                gamma.StandardDeviation = stdev;
                rs.Push(gamma.Sample(defaultrs));//yy
            }
            PrintResult.CompareMeanAndVariance("gamma", mean, stdev * stdev, rs.Mean(), rs.Variance()); // TODO: result not consistent need to fix the bug
        }
        [TestMethod]
        public void TestMeanAndVariacneConsistency_Shape()
        {
            const int numSamples = 100000;
            double mean, stdev;
            double alpha, beta;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Gamma gamma = new Gamma();
            rs.Clear();
            alpha = 4; beta = 4;
            mean = alpha / beta; stdev = Math.Sqrt(alpha / beta / beta);

            for (int i = 0; i < numSamples; ++i)
            {

                gamma.Mean = mean;
                gamma.StandardDeviation = stdev;
                rs.Push(gamma.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("gamma", mean, stdev * stdev, rs.Mean(), rs.Variance()); // TODO: result not consistent need to fix the bug
        }
    }
}
