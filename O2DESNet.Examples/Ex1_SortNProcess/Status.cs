using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Examples.Ex1_SortNProcess
{
    public class Status : Status<Scenario>
    {
        public Processor<Load> P_Sorting { get; private set; } // u 10,2 min
        public Processor<Load> P_Preparing { get; private set; } // 100 sec
        public Processor<Load> P_Processing { get; private set; }  // u 8,5 min
        public Processor<Load> P_Unloading { get; private set; } // 20 sec

        public Status(Scenario scenario, int seed = 0): base(scenario, seed)
        {
            P_Sorting = new Processor<Load>();
            P_Preparing = new Processor<Load>();
            P_Processing = new Processor<Load>();
            P_Unloading = new Processor<Load>();
        }

        public override void WarmedUp(DateTime clockTime)
        {
            P_Sorting.WarmedUp(clockTime);
            P_Preparing.WarmedUp(clockTime);
            P_Processing.WarmedUp(clockTime);
            P_Unloading.WarmedUp(clockTime);
        }
    }
}
