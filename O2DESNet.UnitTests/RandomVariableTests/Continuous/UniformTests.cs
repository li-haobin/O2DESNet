using NUnit.Framework;

using O2DESNet.RandomVariables.Continuous;

using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Continuous;

[TestFixture]
public class UniformTests
{
    [Test]
    public void TestMeanAndVarianceConsistency()
    {
        const int numSamples = 100000;
        double mean, stdev;
        RunningStat rs = new();
        Random defaultrs = new();
        Uniform uniform = new();
        rs.Clear();
        var a = uniform.UpperBound;
        var b = uniform.LowerBound;
        mean = (a + b) / 2;
        stdev = Math.Sqrt((b - a) * (b - a) / 12);
        for (int i = 0; i < numSamples; ++i)
        {

            rs.Push(uniform.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("uniform", mean, stdev * stdev, rs.Mean(), rs.Variance());
    }

    [Test]
    public void IfLowerBoundLarger()
    {
        Random rs = new();
        Uniform uniform = new();
        uniform.UpperBound = 12;
        uniform.LowerBound = 11;
        Assert.That(uniform.UpperBound, Is.EqualTo(12));
        uniform.UpperBound = 10;
        Assert.That(uniform.UpperBound, Is.EqualTo(10));
        TestContext.Out.WriteLine(" " + uniform.UpperBound);
        uniform.LowerBound = 13;
        Assert.That(uniform.UpperBound, Is.EqualTo(uniform.LowerBound));
        TestContext.Out.WriteLine(uniform.Sample(rs));
        TestContext.Out.WriteLine(uniform.UpperBound);
        TestContext.Out.WriteLine(uniform.LowerBound);
    }
}
