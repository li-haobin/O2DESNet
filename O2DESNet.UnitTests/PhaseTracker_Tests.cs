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

            var checkpoint = DateTime.MinValue.AddMinutes(3);
            AssertProportion(pr, "Idle", checkpoint, 1.6 / 3);
            AssertProportion(pr, "Busy1", checkpoint, 0.8 / 3);
            AssertProportion(pr, "Busy2", checkpoint, 0.6 / 3);
            AssertProportion(pr, "Other", checkpoint, 0);
        }

        [Test]
        public void PhaseTracer_at_Non_MinDateTime()
        {
            var pr = new PhaseTracer("Idle", new DateTime(1, 1, 1, 0, 1, 0));
            pr.UpdPhase("Busy1", DateTime.MinValue.AddMinutes(1.2));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2));
            pr.UpdPhase("Idle", DateTime.MinValue.AddMinutes(2.5));
            pr.UpdPhase("Busy2", DateTime.MinValue.AddMinutes(2.9));

            var checkpoint = DateTime.MinValue.AddMinutes(3);
            AssertProportion(pr, "Idle", checkpoint, 0.6 / 2);
            AssertProportion(pr, "Busy1", checkpoint, 0.8 / 2);
            AssertProportion(pr, "Busy2", checkpoint, 0.6 / 2);
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

            var checkpoint = DateTime.MinValue.AddMinutes(3);
            AssertProportion(pr, "Idle", checkpoint, 0.4 / 1.5);
            AssertProportion(pr, "Busy1", checkpoint, 0.5 / 1.5);
            AssertProportion(pr, "Busy2", checkpoint, 0.6 / 1.5);
        }

        private static void AssertProportion(PhaseTracer tracer, string phase, DateTime checkpoint, double expected)
        {
            var actual = tracer.GetProportion(phase, checkpoint);
            const double tolerance = 1e-9;
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance), $"{phase} proportion mismatch");
        }
    }
}
