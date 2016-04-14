using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Warehouse.Statics;

namespace O2DESNet.Warehouse
{
    public class ExperimentFramework
    {


        public void ExperimentRunAllStrategies(string orderFilename)
        {
            WarehouseSim whsim = null;
            var AllStrategies = Enum.GetValues(typeof(PicklistGenerator.Strategy));

            int NumRuns = IOHelper.GetNumRuns("ZA");
            IOHelper.ClearOutputFiles("ZA");

            for (int runID = 1; runID <= NumRuns; runID++)
            {
                //Parallel.ForEach(AllStrategies, ((PicklistGenerator.Strategy)strategy)=>
                foreach (PicklistGenerator.Strategy strategy in AllStrategies)
                {
                    Console.WriteLine("Running Scenario {0} Strategy {1} ...", runID, strategy.ToString());
                    whsim = null;
                    whsim = new WarehouseSim("ZA", strategy, runID, orderFilename);
                    whsim.Run(24);
                    whsim.PrintStatistics();
                    //whsim.OutputRacks();
                    IOHelper.AddOutputFile(whsim);
                }
                IOHelper.WriteOutputFile(whsim);
                //Debug
                IOHelper.WriteNumOrderFile(whsim);

                whsim = null; // clear memory
            }
            //);
        }

        public void ExperimentSelectStrategy()
        {
            WarehouseSim whsim;

            Console.Write("Strategy to implement? (A/B/C/D) ");
            var strat = Console.ReadLine();

            if (strat[0] == 'A' || strat[0] == 'a') whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.A);
            else if (strat[0] == 'B' || strat[0] == 'b') whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.B);
            else if (strat[0] == 'C' || strat[0] == 'c') whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.C);
            else if (strat[0] == 'D' || strat[0] == 'd') whsim = new WarehouseSim("ZA", PicklistGenerator.Strategy.D);
            else throw new Exception("No such strategy!");

            whsim.Run(24);
            whsim.PrintStatistics();
            IOHelper.AddOutputFile(whsim);
            IOHelper.WriteOutputFile(whsim);
        }
    }
}
