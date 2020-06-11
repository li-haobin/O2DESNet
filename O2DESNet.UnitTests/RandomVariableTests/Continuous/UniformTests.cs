using Microsoft.VisualStudio.TestTools.UnitTesting;
using O2DESNet.RandomVariables.Continuous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous
{
    [TestClass]
    public class UniformTests
    {
        [TestMethod]
        public void TestMeanAndVariacneConsistency()
        {
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Uniform uniform = new Uniform();
            rs.Clear();
            var a = uniform.UpperBound;
            var b = uniform.LowerBound;
            mean = (a + b) / 2; stdev = Math.Sqrt((b - a) * (b - a) / 12);
            for (int i = 0; i < numSamples; ++i)
            {

                rs.Push(uniform.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("uniform", mean, stdev * stdev, rs.Mean(), rs.Variance());
        }
        [TestMethod]
        public void IfLowerBoundLarger()
        {
            Random rs = new Random();
            Uniform uniform = new Uniform();
            uniform.UpperBound = 12;
            uniform.LowerBound = 11;
            Assert.AreEqual(uniform.UpperBound, 12);
            uniform.UpperBound = 10;
            Assert.AreEqual(uniform.UpperBound,10);
            Debug.WriteLine(" " + uniform.UpperBound);
            uniform.LowerBound = 13;
            Assert.AreEqual(uniform.UpperBound,uniform.LowerBound);
            Debug.WriteLine(uniform.Sample(rs));
            Debug.WriteLine(uniform.UpperBound);
            Debug.WriteLine(uniform.LowerBound);
        }
    }
}
