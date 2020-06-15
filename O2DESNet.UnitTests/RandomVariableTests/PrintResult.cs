using System.Diagnostics;

namespace O2DESNet.UnitTests.RandomVariableTests
{
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
            Debug.WriteLine("Testing {0}", name);
            Debug.WriteLine("Expected mean:     {0}, computed mean:     {1}", expectedMean, computedMean);
            Debug.WriteLine("Expected variance: {0}, computed variance: {1}", expectedVariance, computedVariance);
            Debug.WriteLine("");
        }
    }
}
