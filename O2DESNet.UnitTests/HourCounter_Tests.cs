using NUnit.Framework;
using System;

namespace O2DESNet.UnitTests
{
    public class HourCounterTests
    {
        [Test]
        public void Pause_Should_Not_Affect_Update_Last_Count()
        {
            var sb = new TestSandbox();
            var hc = sb.HC;
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.Pause();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(2);
            sb.Run(TimeSpan.FromHours(1));
            hc.Resume();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(5));
            hc.ObserveCount(0);
            if (Math.Abs(hc.AverageCount - 0.375) > 1e-16) Assert.Fail();
            sb.Dispose();
        }

        [Test]
        public void Pause_Should_Affect_Total_Increment()
        {
            var sb = new TestSandbox();
            var hc = sb.HC;
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.Pause();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(2);
            sb.Run(TimeSpan.FromHours(1));
            hc.Resume();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(5));
            hc.ObserveCount(0);
            if (Math.Abs(hc.TotalIncrement - 1) > 1e-16) Assert.Fail();
            if (Math.Abs(hc.TotalDecrement - 2) > 1e-16) Assert.Fail();
            sb.Dispose();
        }

        [Test]
        public void TotalIncrement_At_ClockTime_Equals_LastTime()
        {
            var sb = new TestSandbox();
            var hc = sb.HC;
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            hc.ObserveChange(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveChange(1);
            hc.ObserveChange(-1);
            if (Math.Abs(hc.TotalIncrement - 3) > 1e-16) Assert.Fail();
            if (Math.Abs(hc.TotalDecrement - 1) > 1e-16) Assert.Fail();
            sb.Dispose();
        }

        [Test]
        public void LogFiles()
        {
            var sb = new TestSandbox();
            var hc = sb.HC;
            hc.LogFile = "hc.csv";
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.Pause();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(2);
            sb.Run(TimeSpan.FromHours(1));
            hc.Resume();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(5));
            hc.ObserveCount(0);
            sb.Dispose();
        }

        [Test]
        public void AverageCount_With_UpdateToClockTime_Count()
        {
            var sb = new TestSandbox();
            var hc = sb.HC;
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.Pause();
            sb.Run(TimeSpan.FromHours(1)); // paused
            hc.ObserveCount(2);
            sb.Run(TimeSpan.FromHours(1)); // paused
            hc.Resume();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(5));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(8));
            if (Math.Abs(hc.AverageCount - 0.375 / 2) > 1e-16) Assert.Fail();
            if (Math.Abs(hc.TotalHours - 16) > 1e-16) Assert.Fail();
            sb.Dispose();
        }

        private class TestSandbox : Sandbox
        {
            public HourCounter HC { get; }

            public TestSandbox() : base(seed: 0, id: null, Pointer.Empty)
            {
                HC = AddHourCounter();
            }
        }
    }
}