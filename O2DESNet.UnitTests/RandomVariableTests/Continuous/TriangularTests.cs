using Microsoft.VisualStudio.TestTools.UnitTesting;
using O2DESNet.RandomVariables.Continuous;
using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous
{
    [TestClass]
    public class TriangularTests
    {
        [TestMethod]
        public void TestMeanAndVariacneConsistency()
        {
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Triangular tri = new Triangular();
            rs.Clear();
            var a = tri.LowerBound; var b = tri.UpperBound; var c = tri.Mode;
            mean = (a + b + c) / 3; stdev = Math.Sqrt((a * a + b * b + c * c - a * b - a * c - b * c) / 18);
            for (int i = 0; i < numSamples; ++i)
            {

                rs.Push(tri.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("Triangular", mean, stdev * stdev, rs.Mean(), rs.Variance());
        }
    }
}
