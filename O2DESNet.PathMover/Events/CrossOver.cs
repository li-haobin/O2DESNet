using O2DESNet.PathMover.Dynamics;
using System;

namespace O2DESNet.PathMover.Events
{
    internal class CrossOver : Event<Scenario, Status>
    {
        public Vehicle Vehicle1 { get; set; }
        public Vehicle Vehicle2 { get; set; }
        protected override void Invoke()
        {
            Status.Count_CrossOvers++;
            if (Status.Display)
            {
                Console.WriteLine("{0}: Cross-Over.", ClockTime);
                Console.WriteLine("--------CONFLICT OF-------");
                Console.WriteLine(Vehicle1);
                Console.WriteLine("-----------OVER-----------");
                Console.WriteLine(Vehicle2);
                Console.WriteLine("---------DISTANCE---------");
                Console.WriteLine("{0:F4} + {1:F4} = {2:F4}\n", Vehicle1.GetDistance(ClockTime), Vehicle2.GetDistance(ClockTime), Vehicle1.GetDistance(ClockTime) + Vehicle2.GetDistance(ClockTime));
            }
            //Console.ReadKey();
        }
    }
}
