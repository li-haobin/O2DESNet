using NUnit.Framework;

using System;

namespace O2DESNet.UnitTests;

[TestFixture]
public class PhaseTracer_Tests
{
    [Test]
    public void PhaseTracer_at_MinDateTime()
    {
        var pr = new PhaseTracer("Idle");
        pr.UpdPhase("Busy1", TimeSpan.FromMinutes(1.2));
        pr.UpdPhase("Busy2", TimeSpan.FromMinutes(2));
        pr.UpdPhase("Idle", TimeSpan.FromMinutes(2.5));
        pr.UpdPhase("Busy2", TimeSpan.FromMinutes(2.9));
        if (Diff(pr.GetProportion("Idle", TimeSpan.FromMinutes(3)), 1.6 / 3))
            Assert.Fail();
        if (Diff(pr.GetProportion("Busy1", TimeSpan.FromMinutes(3)), 0.8 / 3))
            Assert.Fail();
        if (Diff(pr.GetProportion("Busy2", TimeSpan.FromMinutes(3)), 0.6 / 3))
            Assert.Fail();
        if (Diff(pr.GetProportion("Other", TimeSpan.FromMinutes(3)), 0))
            Assert.Fail();
    }

    [Test]
    public void PhaseTracer_at_Non_MinDateTime()
    {
        var pr = new PhaseTracer("Idle", TimeSpan.FromMinutes(1));
        pr.UpdPhase("Busy1", TimeSpan.FromMinutes(1.2));
        pr.UpdPhase("Busy2", TimeSpan.FromMinutes(2));
        pr.UpdPhase("Idle", TimeSpan.FromMinutes(2.5));
        pr.UpdPhase("Busy2", TimeSpan.FromMinutes(2.9));
        if (Diff(pr.GetProportion("Idle", TimeSpan.FromMinutes(3)), 0.6 / 2))
            Assert.Fail();
        if (Diff(pr.GetProportion("Busy1", TimeSpan.FromMinutes(3)), 0.8 / 2))
            Assert.Fail();
        if (Diff(pr.GetProportion("Busy2", TimeSpan.FromMinutes(3)), 0.6 / 2))
            Assert.Fail();
    }

    [Test]
    public void PhaseTracer_with_WarmUp()
    {
        var pr = new PhaseTracer("Idle");
        pr.UpdPhase("Busy1", TimeSpan.FromMinutes(1.2));
        pr.WarmedUp(TimeSpan.FromMinutes(1.5));
        pr.UpdPhase("Busy2", TimeSpan.FromMinutes(2));
        pr.UpdPhase("Idle", TimeSpan.FromMinutes(2.5));
        pr.UpdPhase("Busy2", TimeSpan.FromMinutes(2.9));
        if (Diff(pr.GetProportion("Idle", TimeSpan.FromMinutes(3)), 0.4 / 1.5))
            Assert.Fail();
        if (Diff(pr.GetProportion("Busy1", TimeSpan.FromMinutes(3)), 0.5 / 1.5))
            Assert.Fail();
        if (Diff(pr.GetProportion("Busy2", TimeSpan.FromMinutes(3)), 0.6 / 1.5))
            Assert.Fail();
    }

    private static bool Diff(double x1, double x2, int decimals = 12)
    {
        return Math.Round(x1, decimals) != Math.Round(x2, decimals);
    }
}
