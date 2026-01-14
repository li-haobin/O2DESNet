using NUnit.Framework;

using O2DESNet.HourCounters;

using System;

namespace O2DESNet.UnitTests;

[TestFixture]
public class HourCounter_Tests
{
    [Test]
    public void Pause_Should_Not_Affect_Update_Last_Count()
    {
        TestSandbox sb = new();
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
        if (hc.AverageCount != 0.375)
            Assert.Fail();
        sb.Dispose();
    }

    [Test]
    public void Pause_Should_Affect_Total_Increment()
    {
        TestSandbox sb = new();
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
        if (hc.TotalIncrement != 1)
            Assert.Fail();
        if (hc.TotalDecrement != 2)
            Assert.Fail();
        sb.Dispose();
    }

    [Test]
    public void TotalIncrement_At_ClockTime_Equals_LastTime()
    {
        TestSandbox sb = new();
        var hc = sb.HC;
        sb.Run(TimeSpan.FromHours(1));
        hc.ObserveCount(1);
        hc.ObserveChange(1);
        sb.Run(TimeSpan.FromHours(1));
        hc.ObserveChange(1);
        hc.ObserveChange(-1);
        if (hc.TotalIncrement != 3)
            Assert.Fail();
        if (hc.TotalDecrement != 1)
            Assert.Fail();
        sb.Dispose();
    }

    [Test]
    public void AverageCount_With_UpdateToClockTime_Count()
    {
        TestSandbox sb = new();
        var hc = sb.HC;
        sb.Run(TimeSpan.FromHours(1));
        hc.ObserveCount(1);
        sb.Run(TimeSpan.FromHours(1));
        hc.Pause();
        sb.Run(TimeSpan.FromHours(1));
        /// paused
        hc.ObserveCount(2);
        sb.Run(TimeSpan.FromHours(1));
        /// paused
        hc.Resume();
        sb.Run(TimeSpan.FromHours(1));
        hc.ObserveCount(0);
        sb.Run(TimeSpan.FromHours(5));
        hc.ObserveCount(0);
        sb.Run(TimeSpan.FromHours(8));
        if (hc.AverageCount != 0.375 / 2)
            Assert.Fail();
        if (hc.TotalHours != 16)
            Assert.Fail();
        sb.Dispose();
    }

    internal class TestSandbox : Sandbox
    {
        public HourCounter HC { get; private set; }
        public TestSandbox() : base(id: nameof(TestSandbox), seed: 0)
        {
            TestContext.Out.WriteLine("AddHourCounter");
            HC = AddHourCounter();
        }
    }
}