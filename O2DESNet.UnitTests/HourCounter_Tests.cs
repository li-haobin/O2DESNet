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
            var t = DateTime.MinValue;
            hc.ObserveCount(1, t.AddHours(1));
            hc.Pause(t.AddHours(2));
            hc.ObserveCount(2, t.AddHours(3));
            hc.Resume(t.AddHours(4));
            hc.ObserveCount(0, t.AddHours(5));
            hc.ObserveCount(0, t.AddHours(10));
            if (hc.AverageCount != 0.375) Assert.Fail();
            sb.Dispose();
        }
        [Test]
        public void Pause_Should_Affect_Total_Increment()
        {
            TestSandbox sb = new TestSandbox();
            var hc = sb.HC;
            var t = DateTime.MinValue;
            hc.ObserveCount(1, t.AddHours(1));
            hc.Pause(t.AddHours(2));
            hc.ObserveCount(2, t.AddHours(3));
            hc.Resume(t.AddHours(4));
            hc.ObserveCount(0, t.AddHours(5));
            hc.ObserveCount(0, t.AddHours(10));
            if (hc.TotalIncrement != 1) Assert.Fail();
            if (hc.TotalDecrement != 2) Assert.Fail();
            sb.Dispose();
        }
        [Test]
        public void TotalIncrement_At_ClockTime_Equals_LastTime()
        {
            TestSandbox sb = new TestSandbox();
            var hc = sb.HC;
            var t = DateTime.MinValue;
            hc.ObserveCount(1, t.AddHours(1));
            hc.ObserveChange(1, t.AddHours(1));
            hc.ObserveChange(1, t.AddHours(2));
            hc.ObserveChange(-1, t.AddHours(2));
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
            var t = DateTime.MinValue;
            hc.ObserveCount(1, t.AddHours(1));
            hc.Pause(t.AddHours(2));
            hc.ObserveCount(2, t.AddHours(3));
            hc.Resume(t.AddHours(4));
            hc.ObserveCount(0, t.AddHours(5));
            hc.ObserveCount(0, t.AddHours(10));
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