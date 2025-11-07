using NUnit.Framework;

using O2DESNet.RandomVariables.Discrete;

using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Discrete;

[TestFixture]
public class PoissonTests
{
    [Test]
    public void TestMeanAndVariacneConsistency()
    {
        const int numSamples = 100000;
        double mean, stdev;
        RunningStat rs = new();
        Random defaultrs = new();
        Poisson poisson = new();
        rs.Clear();
        mean = 2000;
        stdev = Math.Sqrt(2000);
        poisson.Lambda = 2000;
        for (int i = 0; i < numSamples; ++i)
        {

            rs.Push(poisson.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("Poisson Discrete", mean, stdev * stdev, rs.Mean(), rs.Variance());
    }
}
