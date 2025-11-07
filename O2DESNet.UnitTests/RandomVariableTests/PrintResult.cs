using NUnit.Framework;

namespace O2DESNet.UnitTests.RandomVariableTests;

public static class PrintResult
{
    public static void CompareMeanAndVariance
    (
        string name,
        double expectedMean,
        double expectedVariance,
        double computedMean,
        double computedVariance
    )
    {
        TestContext.Out.WriteLine("Testing {0}", name);
        TestContext.Out.WriteLine("Expected mean:     {0}, computed mean:     {1}", expectedMean, computedMean);
        TestContext.Out.WriteLine("Expected variance: {0}, computed variance: {1}", expectedVariance, computedVariance);
        TestContext.Out.WriteLine("");
    }
}
