using NUnit.Framework;

using O2DESNet.RandomVariables.Categorical;

using System;
using System.Collections.Generic;

namespace O2DESNet.UnitTests.RandomVariableTests.Categorical
{
    [TestFixture]
    public class UniformTests
    {
        [Test]
        public void TestMeanAndVariacneConsistency()
        {
            List<int> numList = [1, 2, 3, 4, 5, 6, 7, 8, 9];
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new();
            Random defaultrs = new();
            Uniform<int> uniform = new();
            uniform.Candidates = numList;
            rs.Clear();
            mean = 50;
            stdev = 0;
            for (int i = 0; i < numSamples; ++i)
            {
                rs.Push(uniform.Sample(defaultrs));
            }

            PrintResult.CompareMeanAndVariance("uniform categorical", mean, stdev * stdev, rs.Mean(), rs.Variance());
        }

        [Test]
        public void TestUniformRVCategoricalGenericObjectSampleMethod()
        {
            List<int> numList = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
            Uniform<int> uniform = new();
            Random rs = new();
            uniform.Candidates = numList;
            for (int i = 0; i < 20; i++)
            {
                var tmep = uniform.Sample(rs);
                TestContext.Out.WriteLine(tmep);
            }
        }

        [Test]
        public void TestUniformRVCategoricalCostumizedObjectSampleMethod()
        {
            Random rs = new();
            List<student> students = [];
            for (int i = 0; i < 20; i++)
            {
                var s = new student();
                s.id = i + 1;
                s.name = "a" + Convert.ToString(i);
                students.Add(s);
            }

            Uniform<student> uniform = new();
            uniform.Candidates = students;
            for (int i = 0; i < 20; i++)
            {
                var temp = uniform.Sample(rs);
                TestContext.Out.WriteLine(temp.name + " " + temp.id);
            }
        }
    }
}

public class student
{
    public int id { get; set; }
    public string name { get; set; }
}
