using NUnit.Framework;
using System;

namespace O2DESNet.UnitTests
{
    public class PhaseTracer_Tests
    {
        [Test]
        public void PhaseTracer_at_MinDateTime()
        {
            var pr = new PhaseTracer("Idle");
            pr.UpdPhase("Busy1", DateTime.MinValue.AddMinutes(1.2));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2));
            pr.UpdPhase("Idle", DateTime.MinValue.AddMinutes(2.5));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2.9));
            if (Diff(pr.GetProportion("Idle", DateTime.MinValue.AddMinutes(3)), 1.6 / 3)) Assert.Fail();
            if (Diff(pr.GetProportion("Busy1", DateTime.MinValue.AddMinutes(3)), 0.8 / 3)) Assert.Fail();
            if (Diff(pr.GetProportion("Busy2", DateTime.MinValue.AddMinutes(3)), 0.6 / 3)) Assert.Fail();
            if (Diff(pr.GetProportion("Other", DateTime.MinValue.AddMinutes(3)), 0)) Assert.Fail();
        }

        [Test]
        public void PhaseTracer_at_Non_MinDateTime()
        {
            var pr = new PhaseTracer("Idle", new DateTime(1, 1, 1, 0, 1, 0));
            pr.UpdPhase("Busy1", DateTime.MinValue.AddMinutes(1.2));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2));
            pr.UpdPhase("Idle", DateTime.MinValue.AddMinutes(2.5));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2.9));
            if (Diff(pr.GetProportion("Idle", DateTime.MinValue.AddMinutes(3)), 0.6 / 2)) Assert.Fail();
            if (Diff(pr.GetProportion("Busy1", DateTime.MinValue.AddMinutes(3)), 0.8 / 2)) Assert.Fail();
            if (Diff(pr.GetProportion("Busy2", DateTime.MinValue.AddMinutes(3)), 0.6 / 2)) Assert.Fail();
        }

        [Test]
        public void PhaseTracer_with_WarmUp()
        {
            var pr = new PhaseTracer("Idle");
            pr.UpdPhase("Busy1", DateTime.MinValue.AddMinutes(1.2));
            pr.WarmedUp(DateTime.MinValue.AddMinutes(1.5));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2));
            pr.UpdPhase("Idle", DateTime.MinValue.AddMinutes(2.5));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2.9));
            if (Diff(pr.GetProportion("Idle", DateTime.MinValue.AddMinutes(3)), 0.4 / 1.5)) Assert.Fail();
            if (Diff(pr.GetProportion("Busy1", DateTime.MinValue.AddMinutes(3)), 0.5 / 1.5)) Assert.Fail();
            if (Diff(pr.GetProportion("Busy2", DateTime.MinValue.AddMinutes(3)), 0.6 / 1.5)) Assert.Fail();
        }

        private static bool Diff(double x1, double x2, int decimals = 12)
        {
            return Math.Round(x1, decimals) != Math.Round(x2, decimals);
        }
    }
}
