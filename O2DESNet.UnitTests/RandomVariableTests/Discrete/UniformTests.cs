using NUnit.Framework;
using O2DESNet.RandomVariables.Discrete;
using System;
using System.Diagnostics;

namespace O2DESNet.UnitTests.RandomVariableTests.Discrete
{
    [TestFixture]
    public class UniformTests
    {
        [Test]
        public void TestMeanAndVariacneConsistency()
        {
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Uniform uniform = new Uniform();

            rs.Clear();
            var a = Convert.ToDouble(uniform.UpperBound);
            var b = Convert.ToDouble(uniform.LowerBound);
            mean = (a + b) / 2; stdev = Math.Sqrt(0.25);
            for (int i = 0; i < numSamples; ++i)
            {

                rs.Push(uniform.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("uniform", mean, stdev * stdev, rs.Mean(), rs.Variance());
            Assert.IsTrue(Math.Abs(mean - rs.Mean()) < 0.1);
            Assert.IsTrue(Math.Abs(stdev * stdev - rs.Variance()) < 0.1);
        }

        [Test]
        public void TestGetterOfMeanAndVariance()
        {
            Uniform uniform = new Uniform();
            Debug.WriteLine(uniform.Mean);
            Debug.WriteLine(uniform.StandardDeviation);
            Assert.AreEqual(uniform.Mean, 0.5);
            Assert.AreEqual(uniform.StandardDeviation, 0.5);
        }

    }
}
