using NUnit.Framework;

using O2DESNet.RandomVariables.Continuous;

using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous;

[TestFixture]
public class NormalTests
{
    [Test]
    public void TestMeanAndVariacneConsistency_Std()
    {
        const int numSamples = 100000;
        double mean, stdev;
        RunningStat rs = new();
        Random defaultrs = new();
        Normal normal = new();
        rs.Clear();
        mean = 2;
        stdev = 5;
        for (int i = 0; i < numSamples; ++i)
        {

            normal.Mean = mean;
            normal.StandardDeviation = stdev;
            rs.Push(normal.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("normal", mean, stdev * stdev, rs.Mean(), rs.Variance());
    }
    [Test]
    public void TestMeanAndVariacneConsistency_CV()
    {
        const int numSamples = 100000;
        double mean, stdev, cv;
        RunningStat rs = new();
        Random defaultrs = new();
        Normal normal = new();
        rs.Clear();
        mean = 2;
        stdev = 5;
        cv = stdev / Math.Abs(mean);
        for (int i = 0; i < numSamples; ++i)
        {

            normal.Mean = mean;
            normal.CV = cv;
            rs.Push(normal.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("normal", mean, stdev * stdev, rs.Mean(), rs.Variance());
    }
}
