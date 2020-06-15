using Microsoft.VisualStudio.TestTools.UnitTesting;
using O2DESNet.RandomVariables.Categorical;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace O2DESNet.UnitTests.RandomVariableTests.Categorical
{
    [TestClass]
    public class UniformTests
    {
        [TestMethod]
        public void TestMeanAndVariacneConsistency()
        {
            List<int> numList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            const int numSamples = 100000;
            double mean, stdev;
            RunningStat rs = new RunningStat();
            Random defaultrs = new Random();
            Uniform<int> uniform = new Uniform<int>();
            uniform.Candidates = numList;
            rs.Clear();
            mean = 50; stdev = 0;
            for (int i = 0; i < numSamples; ++i)
            {
                rs.Push(uniform.Sample(defaultrs));
            }
            PrintResult.CompareMeanAndVariance("uniform categorical", mean, stdev * stdev, rs.Mean(), rs.Variance());
        }
        [TestMethod]
        public void TestUniformRVCategoricalGenericObjectSampleMethod()
        {
            List<int> numList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Uniform<int> uniform = new Uniform<int>();
            Random rs = new Random();
            uniform.Candidates = numList;
            for (int i = 0; i < 20; i++)
            {
                var tmep = uniform.Sample(rs);
                Debug.WriteLine(tmep);
            }
        }
        [TestMethod]
        public void TestUniformRVCategoricalCostumizedObjectSampleMethod()
        {
            Random rs = new Random();
            List<student> students = new List<student>();
            for (int i = 0; i < 20; i++)
            {
                var s = new student();
                s.id = i + 1;
                s.name = "a" + Convert.ToString(i);
                students.Add(s);
            }
            Uniform<student> uniform = new Uniform<student>();
            uniform.Candidates = students;
            for (int i = 0; i < 20; i++)
            {
                var temp = uniform.Sample(rs);
                Debug.WriteLine(temp.name + " " + temp.id);
            }
        }
    }
}

public class student
{
    public int id { get; set; }
    public string name { get; set; }

}
