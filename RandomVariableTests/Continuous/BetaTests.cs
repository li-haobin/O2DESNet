using Microsoft.VisualStudio.TestTools.UnitTesting;
using O2DESNet.RandomVariables.Continuous;
using System;

namespace RandomVariableTests.Continuous
{
    [TestClass]
    public class BetaTests
    {
        [TestMethod]
        public void TestMeanAndVariacneConsistency()
        {
            const int numSamples = 100000;
            double mean, variance;

            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Beta beta = new Beta();
            rs.Clear();
            //double a = 2, b = 70;
            double a = 2, b = 10;
            mean = a / (a + b);
            variance = a * b / (a + b) / (a + b) / (a + b + 1);
            beta.Mean = mean;
            beta.STD = Math.Sqrt(variance);
            for (int i = 0; i < numSamples; ++i)
            {
                rs.Push(beta.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("Beta", mean, variance, rs.Mean(), rs.Variance());
        }

        [TestMethod]
        public void TestMeanAndVariacneConsistency_Mean()
        {
            const int numSamples = 100000;
            double mean, variance;

            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Beta beta = new Beta();
            rs.Clear();
            //double a = 2, b = 70;

            //mean = a / (a + b);
            //variance = mean * (1 - mean) / (a + b + 1);
            mean = .85; variance = 0.5 * 0.5;
            for (int i = 0; i < numSamples; ++i)
            {

                beta.Mean = mean;
                beta.STD = Math.Sqrt(variance);
                rs.Push(beta.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("Beta", mean, variance, rs.Mean(), rs.Variance());
        }

    }

}
