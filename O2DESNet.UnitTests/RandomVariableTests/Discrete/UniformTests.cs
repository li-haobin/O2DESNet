using NUnit.Framework;

using O2DESNet.RandomVariables.Discrete;

using System;

namespace O2DESNet.UnitTests.RandomVariableTests.Discrete;

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

        var a = Convert.ToDouble(uniform.UpperBound);
        var b = Convert.ToDouble(uniform.LowerBound);
        mean = (a + b) / 2;
        stdev = Math.Sqrt(0.25);

        for (int i = 0; i < numSamples; ++i)
        {

            rs.Push(uniform.Sample(defaultrs));
        }

        PrintResult.CompareMeanAndVariance("uniform", mean, stdev * stdev, rs.Mean(), rs.Variance());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Math.Abs(mean - rs.Mean()), Is.LessThan(0.1));
            Assert.That(Math.Abs(stdev * stdev - rs.Variance()), Is.LessThan(0.1));
        }
    }

    [Test]
    public void TestGetterOfMeanAndVariance()
    {
        Uniform uniform = new();
        TestContext.Out.WriteLine(uniform.Mean);
        TestContext.Out.WriteLine(uniform.StandardDeviation);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(uniform.Mean, Is.EqualTo(0.5));
            Assert.That(uniform.StandardDeviation, Is.EqualTo(0.5));
        }
    }
}
