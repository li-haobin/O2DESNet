using NUnit.Framework;
using O2DESNet.Demos;
using System;
using System.Diagnostics;

namespace O2DESNet.UnitTests
{
    public class TandemQueue_Tests
    {
        [Test]
        public void Test1()
        {
            for (int seed = 0; seed < 3; seed++)
            {
                var q = new TandemQueue(3, 5, 5, 2, seed);
                var sw = new Stopwatch();
                sw.Start();
                q.WarmUp(TimeSpan.FromHours(1000));
                q.Run(TimeSpan.FromHours(20000));
                sw.Stop();
                Debug.WriteLine("q1:{0:F4}\tq2:{1:F4}\ts1:{2:F4}\ts2:{3:F4}\tT:{4:F2}hrs\t{5}ms",
                    q.AvgNQueueing1, q.AvgNQueueing2, q.AvgNServing1, q.AvgNServing2,
                    q.AvgHoursInSystem, sw.ElapsedMilliseconds);
            }
        }
    }
}
