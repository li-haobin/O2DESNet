using MathNet.Numerics.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using O2DESNet.RandomVariables.Discrete;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RandomVariableTests.Discrete
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
            //REngine engine;

            //REngine.SetEnvironmentVariables();
            //engine = REngine.GetInstance();
            //engine.Initialize();

            //rs.Clear();
            //uniform.LowerBound = 1; uniform.UpperBound = 9;
            //uniform.IncludeBound = true;
            //engine.Evaluate("x4 <- sample(" + uniform.LowerBound + ":" + uniform.UpperBound + ", " + numSamples + ", replace = T)");

            //var meanSample = engine.Evaluate("sampleMean <- mean(x4)").AsNumeric();
            //var stdSample = engine.Evaluate("sampleStd <- sd(x4)").AsNumeric();

            //string getMean = meanSample[0].ToString();
            //string getStd = stdSample[0].ToString();

            //mean = Convert.ToDouble(getMean); stdev = Convert.ToDouble(getStd);

            //for (int i = 0; i < numSamples; ++i)
            //{

            //    rs.Push(uniform.Sample(defaultrs));
            //}
            //PrintResult.CompareMeanAndVariance("uniform", mean, stdev * stdev, rs.Mean(), rs.Variance());

        }

        [TestMethod]
        public void TestGetterOfMeanAndVariance()
        {
            Uniform uniform = new Uniform();
            Debug.WriteLine(uniform.Mean);
            Debug.WriteLine(uniform.StandardDeviation);
            Debug.Assert(uniform.Mean == 0.5);
            Debug.Assert(uniform.StandardDeviation == 0.5);
        }

    }
}
