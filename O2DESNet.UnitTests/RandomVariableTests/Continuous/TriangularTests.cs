using NUnit.Framework;

using O2DESNet.RandomVariables.Continuous;

using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous;

[TestFixture]
public class TriangularTests
{
    [Test]
    public void TestMeanAndVariacneConsistency()
    {
        const int numSamples = 100000;
        double mean, stdev;
        RunningStat rs = new();
        Random defaultrs = new();
        Triangular tri = new();
        rs.Clear();
        var a = tri.LowerBound;
        var b = tri.UpperBound;
        var c = tri.Mode;
        mean = (a + b + c) / 3;
        stdev = Math.Sqrt((a * a + b * b + c * c - a * b - a * c - b * c) / 18);
        for (int i = 0; i < numSamples; ++i)
        {

            rs.Push(tri.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("Triangular", mean, stdev * stdev, rs.Mean(), rs.Variance());
    }
}
