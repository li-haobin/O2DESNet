using NUnit.Framework;
using O2DESNet.RandomVariables.Categorical;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace O2DESNet.UnitTests.RandomVariableTests.Categorical
{
    [TestFixture]
    public class UniformTests
    {
        [Test]
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
        [Test]
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
        [Test]
        public void TestUniformRVCategoricalCostumizedObjectSampleMethod()
        {
            Random rs = new Random();
            var students = new List<Student>();
            for (int i = 0; i < 20; i++)
            {
                var s = new Student { Id = i + 1, Name = "a" + i };
                students.Add(s);
            }
            var uniform = new Uniform<Student> { Candidates = students };
            for (int i = 0; i < 20; i++)
            {
                var temp = uniform.Sample(rs);
                Debug.WriteLine(temp.Name + " " + temp.Id);
            }
        }
    }
}

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
