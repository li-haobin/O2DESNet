using NUnit.Framework;
using System;

namespace O2DESNet.UnitTests
{
    public class HourCounter_Tests
    {
        [Test]
        public void Pause_Should_Not_Affect_Update_Last_Count()
        {
            TestSandbox sb = new TestSandbox();
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
            if (hc.AverageCount != 0.375) Assert.Fail();
            sb.Dispose();
        }
        [Test]
        public void Pause_Should_Affect_Total_Increment()
        {
            TestSandbox sb = new TestSandbox();
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
            if (hc.TotalIncrement != 1) Assert.Fail();
            if (hc.TotalDecrement != 2) Assert.Fail();
            sb.Dispose();
        }
        [Test]
        public void TotalIncrement_At_ClockTime_Equals_LastTime()
        {
            TestSandbox sb = new TestSandbox();
            var hc = sb.HC;
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            hc.ObserveChange(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveChange(1);
            hc.ObserveChange(-1);
            if (hc.TotalIncrement != 3) Assert.Fail();
            if (hc.TotalDecrement != 1) Assert.Fail();
            sb.Dispose();
        }
        [Test]
        public void LogFiles()
        {
            TestSandbox sb = new TestSandbox();
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
            TestSandbox sb = new TestSandbox();
            var hc = sb.HC;
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(1);
            sb.Run(TimeSpan.FromHours(1));
            hc.Pause();
            sb.Run(TimeSpan.FromHours(1)); /// paused
            hc.ObserveCount(2);
            sb.Run(TimeSpan.FromHours(1)); /// paused
            hc.Resume();
            sb.Run(TimeSpan.FromHours(1));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(5));
            hc.ObserveCount(0);
            sb.Run(TimeSpan.FromHours(8));
            if (hc.AverageCount != 0.375 / 2) Assert.Fail();
            if (hc.TotalHours != 16) Assert.Fail();
            sb.Dispose();
        }

        class TestSandbox : Sandbox
        {
            public HourCounter HC { get; private set; }
            public TestSandbox()
            {
                HC = AddHourCounter();
            }
        }
    }
}