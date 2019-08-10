using NUnit.Framework;
using O2DESNet.Standard;
using System;

namespace O2DESNet.UnitTests
{
    public class Simulator_Tests
    {
        [Test]
        public void ClockTime_Advance()
        {
            using (var sim = new Server(new Server.Statics { Capacity = 1 }, 0))
            {
                sim.Run(TimeSpan.FromHours(2));
                if (sim.ClockTime != new DateTime(1, 1, 1, 2, 0, 0)) Assert.Fail();
            }
        }
    }
}
