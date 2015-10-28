using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Events
{
    internal class CrossOver : Event
    {
        internal Vehicle Vehicle1 { get; private set; }
        internal Vehicle Vehicle2 { get; private set; }
        internal CrossOver(Simulator sim, Vehicle vehicle1, Vehicle vehicle2) : base(sim) { Vehicle1 = vehicle1; Vehicle2 = vehicle2; }
        public override void Invoke()
        {
            _sim.Status.Count_CrossOvers++;
            if (false)
            {
                Console.WriteLine("{0}: Cross-Over.", _sim.ClockTime);
                Console.WriteLine("--------CONFLICT OF-------");
                Console.WriteLine(Vehicle1);
                Console.WriteLine("-----------OVER-----------");
                Console.WriteLine(Vehicle2);
                Console.WriteLine("---------DISTANCE---------");
                Console.WriteLine("{0:F4} + {1:F4} = {2:F4}\n", Vehicle1.GetDistance(_sim.ClockTime), Vehicle2.GetDistance(_sim.ClockTime), Vehicle1.GetDistance(_sim.ClockTime) + Vehicle2.GetDistance(_sim.ClockTime));
            }
            //Console.ReadKey();
        }
    }
}
