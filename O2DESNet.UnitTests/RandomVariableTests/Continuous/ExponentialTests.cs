using NUnit.Framework;

using O2DESNet.RandomVariables.Continuous;

using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous;

[TestFixture]
public class ExponentialTests
{
    [Test]
    public void TestMeanAndVariacneConsistency()
    {
        const int numSamples = 100000;
        double mean, stdev;
        RunningStat rs = new();
        Random defaultrs = new();
        Exponential exponential = new();
        rs.Clear();
        mean = 2;
        stdev = 2;
        for (int i = 0; i < numSamples; ++i)
        {
            exponential.StandardDeviation = 2;
            //exponential.Mean = mean;
            rs.Push(exponential.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("exponential", mean, stdev * stdev, rs.Mean(), rs.Variance());
    }
}
